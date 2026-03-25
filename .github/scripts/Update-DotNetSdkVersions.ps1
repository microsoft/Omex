<#
.SYNOPSIS
    Updates .NET SDK and MSBuild SDK versions in global.json file.

.DESCRIPTION
    Updates the .NET SDK version by querying the latest release from aka.ms redirects,
    and updates MSBuild SDK package versions using 'dotnet package search'.

.PARAMETER GlobalJsonPath
    Relative path to the global.json file from the source directory.
    Default: "global.json"

.PARAMETER SdkChannel
    The .NET SDK release channel to track. Options: STS, LTS, 8.0, 9.0, etc.
    Default: "LTS"

.PARAMETER SourcesDirectory
    The root source directory containing the global.json file.
    Default: $env:BUILD_SOURCESDIRECTORY (Azure Pipelines) or $env:GITHUB_WORKSPACE (GitHub Actions) or current directory

.PARAMETER FailOnError
    If $true, throws an exception on SDK lookup failures. If $false, logs warnings and continues.
    Default: $false

.NOTES
    Verbose logging is automatically enabled when Azure Pipelines System.Debug is set to 'true'.
    To enable verbose logging, set the system.debug variable in your pipeline or run with:
    variables:
      system.debug: true

.EXAMPLE
    .\Update-DotNetSdkVersions.ps1

.EXAMPLE
    .\Update-DotNetSdkVersions.ps1 -SdkChannel "LTS" -FailOnError $true

.EXAMPLE
    $env:SYSTEM_DEBUG = 'true'; .\Update-DotNetSdkVersions.ps1 -GlobalJsonPath "global.json"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$GlobalJsonPath = "global.json",

    [Parameter(Mandatory = $false)]
    [string]$SdkChannel = "LTS",

    [Parameter(Mandatory = $false)]
    [string]$SourcesDirectory = $(
        if ($env:BUILD_SOURCESDIRECTORY) {
            $env:BUILD_SOURCESDIRECTORY
        }
        elseif ($env:GITHUB_WORKSPACE) {
            $env:GITHUB_WORKSPACE
        }
        else {
            (Get-Location).Path
        }
    ),

    [Parameter(Mandatory = $false)]
    [bool]$FailOnError = $false,

    # (Nuget Config to use)
    [Parameter(Mandatory = $false)]
    [string]$NugetConfigPath = "NuGet.Config"
)

# Normalize and validate SourcesDirectory so it is always a valid root directory
if ([string]::IsNullOrWhiteSpace($SourcesDirectory)) {
    # GitHub Actions default
    $SourcesDirectory = $env:GITHUB_WORKSPACE
}

if ([string]::IsNullOrWhiteSpace($SourcesDirectory)) {
    # Local or generic PowerShell fallback
    $SourcesDirectory = (Get-Location).Path
}

if (-not (Test-Path -LiteralPath $SourcesDirectory -PathType Container)) {
    throw "SourcesDirectory '$SourcesDirectory' does not exist or is not a directory. Specify a valid -SourcesDirectory path."
}

# Resolve to a fully qualified, normalized path
$SourcesDirectory = (Resolve-Path -LiteralPath $SourcesDirectory).ProviderPath

# Determine verbose logging:
# 1. Prefer explicit -Verbose parameter (from CmdletBinding/common parameters)
# 2. Fall back to Azure Pipelines System.Debug variable
if ($PSBoundParameters.ContainsKey('Verbose')) {
    $EnableVerboseLogging = [bool]$PSBoundParameters['Verbose']
}
else {
    $EnableVerboseLogging = ($env:SYSTEM_DEBUG -eq 'true') -or ($env:SYSTEM_DEBUG -eq '1')
}

$ErrorActionPreference = if ($FailOnError) { "Stop" } else { "Continue" }

# Helper function for version comparison
function Get-LatestVersionFromString {
    param (
        [string]$First,
        [string]$Second
    )
    
    if (-not $First) { return $Second }
    if (-not $Second) { return $First }
    
    function Get-VersionFromString {
        param ([string]$Value)
        
        $splitIndex = $Value.IndexOf('-')
        if ($splitIndex -eq -1) {
            $versionString = $Value
            $suffix = ''
        } else {
            $versionString = $Value.Substring(0, $splitIndex)
            $suffix = $Value.Substring($splitIndex)
        }
        
        $version = $null
        if (-not [System.Version]::TryParse($versionString, [ref]$version)) {
            $version = $versionString
        }
        
        return [PSCustomObject]@{ Version = $version; Suffix = $suffix }
    }
    
    function Compare-PreReleaseSuffix {
        param (
            [string]$FirstSuffix,
            [string]$SecondSuffix
        )

        if ($FirstSuffix -eq $SecondSuffix) { return 0 }

        # Trim leading '-' if present
        if ($FirstSuffix.StartsWith('-')) { $FirstSuffix = $FirstSuffix.Substring(1) }
        if ($SecondSuffix.StartsWith('-')) { $SecondSuffix = $SecondSuffix.Substring(1) }

        $firstIdentifiers = $FirstSuffix -split '\.'
        $secondIdentifiers = $SecondSuffix -split '\.'

        $maxLength = [Math]::Max($firstIdentifiers.Length, $secondIdentifiers.Length)

        for ($i = 0; $i -lt $maxLength; $i++) {
            if ($i -ge $firstIdentifiers.Length) { return -1 }  # fewer identifiers => lower precedence
            if ($i -ge $secondIdentifiers.Length) { return 1 }  # more identifiers => higher precedence

            $firstId = $firstIdentifiers[$i]
            $secondId = $secondIdentifiers[$i]

            if ($firstId -eq $secondId) { continue }

            $firstNum = 0
            $secondNum = 0
            $firstIsNumeric = [int]::TryParse($firstId, [ref]$firstNum)
            $secondIsNumeric = [int]::TryParse($secondId, [ref]$secondNum)

            if ($firstIsNumeric -and $secondIsNumeric) {
                if ($firstNum -lt $secondNum) { return -1 }
                if ($firstNum -gt $secondNum) { return 1 }
                continue
            }

            if ($firstIsNumeric -and -not $secondIsNumeric) { return -1 } # numeric < non-numeric
            if (-not $firstIsNumeric -and $secondIsNumeric) { return 1 }  # non-numeric > numeric

            $stringCompare = [string]::CompareOrdinal($firstId, $secondId)
            if ($stringCompare -lt 0) { return -1 }
            if ($stringCompare -gt 0) { return 1 }
        }

        return 0
    }

    $firstVersionObject = Get-VersionFromString $First
    $secondVersionObject = Get-VersionFromString $Second
    
    if ($firstVersionObject.Version -eq $secondVersionObject.Version) {
        if (-not $firstVersionObject.Suffix) { return $First }
        if (-not $secondVersionObject.Suffix) { return $Second }

        $suffixComparison = Compare-PreReleaseSuffix -FirstSuffix $firstVersionObject.Suffix -SecondSuffix $secondVersionObject.Suffix
        if ($suffixComparison -lt 0) { return $Second }
        return $First
    }
    
    if ($firstVersionObject.Version -lt $secondVersionObject.Version) { return $Second }
    return $First
}

function Test-PreReleaseVersion {
    param ([string]$Version)
    return $Version.Contains('-')
}

function Get-LatestSdkVersion {
    param ([string]$Channel = "LTS")
    
    $sdkRedirectUrl = "https://aka.ms/dotnet/$Channel/dotnet-sdk-win-x64.zip"
    
    try {
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Querying SDK redirect URL: $sdkRedirectUrl"
        }
        
        # Follow redirects automatically and get final URL
        $response = Invoke-WebRequest -Uri $sdkRedirectUrl -Method HEAD -MaximumRedirection 10
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Response status: $($response.StatusCode)"
            Write-Host "  [VERBOSE] Response headers: $($response.Headers | ConvertTo-Json -Compress)"
        }
        
        # Determine final URL from response object in a way that works in both
        # PowerShell Core and Windows PowerShell 5.1.
        $finalUrl = $null
        
        # Preferred (PowerShell Core): RequestMessage.RequestUri
        if ($response.BaseResponse -and
            $response.BaseResponse.RequestMessage -and
            $response.BaseResponse.RequestMessage.RequestUri) {
            $finalUrl = $response.BaseResponse.RequestMessage.RequestUri.AbsoluteUri
        }
        
        # Fallback: Location header (some environments expose final URI here)
        if (-not $finalUrl -and $response.Headers -and $response.Headers.Location) {
            $finalUrl = $response.Headers.Location.ToString()
        }
        
        # Fallback (Windows PowerShell 5.1): BaseResponse.ResponseUri
        if (-not $finalUrl -and
            $response.BaseResponse -and
            $response.BaseResponse.ResponseUri) {
            $finalUrl = $response.BaseResponse.ResponseUri.AbsoluteUri
        }
        
        if (-not $finalUrl) {
            throw "Could not determine final redirect URL from response"
        }
        
        Write-Host "Resolved SDK URL: $finalUrl"
        
        # URL format: https://dotnetcli.azureedge.net/dotnet/Sdk/<version>/dotnet-sdk-<version>-win-x64.zip
        # Pattern: \d+ for major/minor to support multi-digit versions (e.g., 10.0.101)
        $version = ($finalUrl | Select-String -Pattern "\d+\.\d+\.\d{3}").Matches.Value
        if (-not $version) {
            throw "Failed to extract version from URL: $finalUrl"
        }
        
        Write-Host "Latest .NET SDK version: $version"
        return $version
    }
    catch {
        $errorMsg = "Failed to retrieve SDK version from ${sdkRedirectUrl}: $($_.Exception.Message)"
        if ($FailOnError) {
            throw $errorMsg
        } else {
            Write-Warning $errorMsg
            return $null
        }
    }
}

function Get-LatestPackageVersion {
    param (
        [string]$PackageId,
        [bool]$IncludePrerelease
    )
    
    try {
        # Check for NuGet configuration files in the sources directory
        $configFilePath = $null

        # Prefer a GitHub-specific config if present, fall back to the repo-wide NuGet.Config
        $nugetGitHubConfigPath = Join-Path $SourcesDirectory $NugetConfigPath
        if (Test-Path $nugetGitHubConfigPath) {
            $configFilePath = $nugetGitHubConfigPath
            if ($EnableVerboseLogging) {
                Write-Host "  [VERBOSE] Using '$NugetConfigPath' from: '$nugetGitHubConfigPath'."
            }
        }
        else {
            Write-Warning "$NugetConfigPath not found under: $SourcesDirectory - search may not find private feeds"
        }

        $dotnetArgs = @(
            'package'
            'search'
            $PackageId
            '--exact-match'
            '--format'
            'json'
        )

        if ($IncludePrerelease) {
            $dotnetArgs += '--prerelease'
        }

        if ($configFilePath) {
            $dotnetArgs += '--configfile'
            $dotnetArgs += $configFilePath
        }

        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Executing: dotnet $($dotnetArgs -join ' ')"
        }
        
        Write-Host "Searching for MSBuild SDK: $PackageId"

        $stderrFile = New-TemporaryFile
        try {
            $outputLines = & dotnet @dotnetArgs 2> $stderrFile
            $output = $outputLines | Out-String

            if ($LASTEXITCODE -ne 0) {
                $errorDetail = ''
                if (Test-Path $stderrFile) {
                    $errorDetail = Get-Content -Path $stderrFile -Raw
                }
                if ([string]::IsNullOrWhiteSpace($errorDetail)) {
                    $errorDetail = $output
                }

                $errorMsg = "Failed to search for package '$PackageId': $errorDetail"
                if ($FailOnError) {
                    throw $errorMsg
                } else {
                    Write-Warning $errorMsg
                    return $null
                }
            }
        
            if ($EnableVerboseLogging) {
                Write-Host "  [VERBOSE] Raw output: $output"
            }
        
            $result = $output | ConvertFrom-Json
        }
        finally {
            if (Test-Path $stderrFile) {
                Remove-Item $stderrFile -ErrorAction SilentlyContinue
            }
        }
        
        if (-not $result.searchResult -or $result.searchResult.Count -eq 0) {
            $errorMsg = "Package '$PackageId' not found in any configured feed"
            if ($FailOnError) {
                throw $errorMsg
            } else {
                Write-Warning $errorMsg
                return $null
            }
        }
        
        # Iterate through all sources and packages to find the latest version
        # The JSON structure is: searchResult[].packages[].version
        $latestVersion = $null
        foreach ($source in $result.searchResult) {
            if ($source.packages) {
                foreach ($package in $source.packages) {
                    if ($package.id -eq $PackageId) {
                        $latestVersion = Get-LatestVersionFromString -First $latestVersion -Second $package.version
                    }
                }
            }
        }
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Found latest version: $latestVersion"
        }
        
        return $latestVersion
    }
    catch {
        $errorMsg = "Error searching for package '$PackageId': $_"
        if ($FailOnError) {
            throw $errorMsg
        } else {
            Write-Warning $errorMsg
            return $null
        }
    }
}

# Main script execution
Write-Host "==============================================================================="
Write-Host ".NET SDK Version Update Script"
Write-Host "==============================================================================="
Write-Host "Global JSON Path: $GlobalJsonPath"
Write-Host "SDK Channel: $SdkChannel"
Write-Host "Sources Directory: $SourcesDirectory"
Write-Host "Verbose Logging: $EnableVerboseLogging"
Write-Host "Fail On Error: $FailOnError"
Write-Host "==============================================================================="

$globalJsonFile = Join-Path $SourcesDirectory $GlobalJsonPath
Write-Host "Full path to global.json: $globalJsonFile"

if (-not (Test-Path $globalJsonFile)) {
    $errorMsg = "global.json file not found at: $globalJsonFile"
    if ($FailOnError) {
        throw $errorMsg
    } else {
        Write-Warning $errorMsg
        exit 0
    }
}

Write-Host "Updating SDK versions in: $globalJsonFile"
Write-Host "Using SDK channel: $SdkChannel"

# Load global.json
$globalJson = Get-Content $globalJsonFile -Raw | ConvertFrom-Json

if ($EnableVerboseLogging) {
    Write-Host "[VERBOSE] Loaded global.json content:"
    Write-Host ($globalJson | ConvertTo-Json -Depth 10)
}

$updateCount = 0

# Update .NET SDK version
$currentSdkVersion = $globalJson.sdk.version
Write-Host "Current .NET SDK version: $currentSdkVersion"

$latestSdkVersion = Get-LatestSdkVersion -Channel $SdkChannel

if ($latestSdkVersion) {
    $selectedVersion = Get-LatestVersionFromString -First $currentSdkVersion -Second $latestSdkVersion
    
    if ($EnableVerboseLogging) {
        Write-Host "[VERBOSE] SDK version comparison: current=$currentSdkVersion, latest=$latestSdkVersion, selected=$selectedVersion"
    }
    
    if ($selectedVersion -ne $currentSdkVersion) {
        Write-Host "##[section]Updating .NET SDK from '$currentSdkVersion' to '$latestSdkVersion'"
        $globalJson.sdk.version = $latestSdkVersion
        $updateCount++
    } else {
        Write-Host ".NET SDK already at latest version '$currentSdkVersion'"
    }
}

# Update MSBuild SDKs
if ($globalJson.'msbuild-sdks') {
    $msbuildSdks = $globalJson.'msbuild-sdks'
    
    if ($EnableVerboseLogging) {
        Write-Host "[VERBOSE] Found $($msbuildSdks.PSObject.Properties.Count) MSBuild SDK(s) to check"
    }
    
    foreach ($property in $msbuildSdks.PSObject.Properties) {
        $packageName = $property.Name
        $currentVersion = $property.Value
        
        Write-Host "Checking MSBuild SDK: $packageName (current: $currentVersion)"
        
        $includePrerelease = Test-PreReleaseVersion -Version $currentVersion
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Include prerelease: $includePrerelease"
        }
        
        $latestVersion = Get-LatestPackageVersion -PackageId $packageName -IncludePrerelease $includePrerelease
        
        if (-not $latestVersion) {
            Write-Host "No update available for '$packageName'"
            continue
        }
        
        $selectedVersion = Get-LatestVersionFromString -First $currentVersion -Second $latestVersion
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Version comparison: current=$currentVersion, latest=$latestVersion, selected=$selectedVersion"
        }
        
        if ($selectedVersion -ne $currentVersion) {
            Write-Host "##[section]Updating MSBuild SDK '$packageName' from '$currentVersion' to '$latestVersion'"
            $msbuildSdks.$packageName = $latestVersion
            $updateCount++
        } else {
            Write-Host "MSBuild SDK '$packageName' already at latest version '$currentVersion'"
        }
    }
}

if ($updateCount -gt 0) {
    Write-Host "##[section]Saving $updateCount SDK updates to $globalJsonFile"
    
    # Save with consistent JSON formatting (2-space indent)
    $globalJson | ConvertTo-Json -Depth 10 | Set-Content $globalJsonFile
    Write-Host "Successfully updated $updateCount SDK(s)"
} else {
    Write-Host "No SDK updates needed"
}

Write-Host "==============================================================================="
Write-Host ".NET SDK Update Complete"
Write-Host "==============================================================================="
