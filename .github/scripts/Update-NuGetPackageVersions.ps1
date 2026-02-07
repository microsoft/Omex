<#
.SYNOPSIS
    Updates NuGet package versions in Directory.Packages.props file.

.DESCRIPTION
    Scans Directory.Packages.props for PackageVersion elements in ItemGroups labeled 'AutoUpdate',
    queries the latest available versions using 'dotnet package search', and updates the Version
    attributes accordingly. Respects PreserveMajor attribute and pre-release version detection.

.PARAMETER PropsFilePath
    Relative path to the Directory.Packages.props file from the source directory.
    Default: "Directory.Packages.props"

.PARAMETER SourcesDirectory
    The root source directory containing the props file. 
    Default: $env:BUILD_SOURCESDIRECTORY (Azure Pipelines variable)

.PARAMETER FailOnError
    If $true, throws an exception on package lookup failures. If $false, logs warnings and continues.
    Default: $false

.NOTES
    Verbose logging is automatically enabled when Azure Pipelines System.Debug is set to 'true'.
    To enable verbose logging, set the system.debug variable in your pipeline or run with:
    variables:
      system.debug: true

.EXAMPLE
    .\Update-NuGetPackageVersions.ps1

.EXAMPLE
    .\Update-NuGetPackageVersions.ps1 -PropsFilePath "Directory.Packages.props" -FailOnError $true

.EXAMPLE
    $env:SYSTEM_DEBUG = 'true'; .\Update-NuGetPackageVersions.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$PropsFilePath = "Directory.Packages.props",

    [Parameter(Mandatory = $false)]
    [string]$SourcesDirectory = $env:BUILD_SOURCESDIRECTORY,

    [Parameter(Mandatory = $false)]
    [bool]$FailOnError = $false
)

# Determine whether verbose logging should be enabled:
# - Prefer the standard -Verbose common parameter when explicitly passed
# - Fall back to Azure Pipelines System.Debug variable for backwards compatibility
$isVerboseParameterSet = $PSBoundParameters.ContainsKey('Verbose') -and $PSBoundParameters['Verbose']
$EnableVerboseLogging = $isVerboseParameterSet -or ($env:SYSTEM_DEBUG -eq 'true') -or ($env:SYSTEM_DEBUG -eq '1')

if ($EnableVerboseLogging) {
    $VerbosePreference = 'Continue'
}
$ErrorActionPreference = if ($FailOnError) { "Stop" } else { "Continue" }

# Helper function for version comparison (from VersionUtils.ps1)
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
    
    $firstVersionObject = Get-VersionFromString $First
    $secondVersionObject = Get-VersionFromString $Second
    
    if ($firstVersionObject.Version -eq $secondVersionObject.Version) {
        if (-not $firstVersionObject.Suffix) { return $First }
        if (-not $secondVersionObject.Suffix) { return $Second }
        if ($firstVersionObject.Suffix -lt $secondVersionObject.Suffix) { return $Second }
        return $First
    }
    
    if ($firstVersionObject.Version -lt $secondVersionObject.Version) { return $Second }
    return $First
}

function Test-PreReleaseVersion {
    param ([string]$Version)
    return $Version.Contains('-')
}

function Get-LatestPackageVersion {
    param (
        [string]$PackageId,
        [bool]$IncludePrerelease,
        [string]$MajorVersion = ""
    )
    
    try {
        $prereleaseFlag = if ($IncludePrerelease) { "--prerelease" } else { "" }
        
        # Check if NuGet.config exists in the sources directory
        $nugetConfigPath = Join-Path $SourcesDirectory "NuGet.config"
        $configSourceFlag = ""
        if (Test-Path $nugetConfigPath) {
            $configSourceFlag = "--configfile `"$nugetConfigPath`""
            if ($EnableVerboseLogging) {
                Write-Host "  [VERBOSE] Using NuGet.config from: $nugetConfigPath"
            }
        } else {
            Write-Warning "NuGet.config not found at: $SourcesDirectory - search may not find private feeds"
        }
        
        $searchCmd = "dotnet package search `"$PackageId`" --exact-match --format json $prereleaseFlag $configSourceFlag"
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Executing: $searchCmd"
        }
        
        Write-Host "Searching for package: $PackageId $(if ($MajorVersion) { "(major version $MajorVersion.*)" })"
        
        $output = Invoke-Expression $searchCmd 2>&1 | Out-String
        
        if ($LASTEXITCODE -ne 0) {
            $errorMsg = "Failed to search for package '$PackageId': $output"
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
                        # If major version filtering is needed, skip non-matching versions
                        if ($MajorVersion -and -not ($package.version -match "^$MajorVersion\.")) {
                            continue
                        }
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
Write-Host "NuGet Package Version Update Script"
Write-Host "==============================================================================="
Write-Host "Props File Path: $PropsFilePath"
Write-Host "Sources Directory: $SourcesDirectory"
Write-Host "Verbose Logging: $EnableVerboseLogging"
Write-Host "Fail On Error: $FailOnError"
Write-Host "==============================================================================="

$propsFile = Join-Path $SourcesDirectory $PropsFilePath
Write-Host "Full path to props file: $propsFile"

if (-not (Test-Path $propsFile)) {
    $errorMsg = "Props file not found at: $propsFile"
    if ($FailOnError) {
        throw $errorMsg
    } else {
        Write-Warning $errorMsg
        exit 1
    }
}

Write-Host "Updating NuGet packages in: $propsFile"

# Load XML with whitespace preservation
$xml = New-Object System.Xml.XmlDocument
$xml.PreserveWhitespace = $true
$xml.Load($propsFile)

$updateCount = 0

# Find all ItemGroups with AutoUpdate label
$autoUpdateGroups = $xml.Project.ItemGroup | Where-Object { 
    $null -ne $_.Label -and $_.Label -match 'AutoUpdate' 
}

Write-Host "Found $($autoUpdateGroups.Count) ItemGroups with AutoUpdate label"

foreach ($itemGroup in $autoUpdateGroups) {
    $packageVersions = $itemGroup.PackageVersion
    
    if (-not $packageVersions) { continue }
    
    if ($EnableVerboseLogging) {
        Write-Host "[VERBOSE] Processing ItemGroup with $(@($packageVersions).Count) packages"
    }
    
    foreach ($packageVersion in $packageVersions) {
        $packageId = $packageVersion.Include
        $currentVersion = $packageVersion.Version
        $preserveMajor = $packageVersion.PreserveMajor -eq "true"
        
        if (-not $packageId -or -not $currentVersion) {
            Write-Host "Skipping invalid PackageVersion: $($packageVersion.OuterXml)"
            continue
        }
        
        if ($EnableVerboseLogging) {
            Write-Host "[VERBOSE] Processing: $packageId (current: $currentVersion, preserveMajor: $preserveMajor)"
        }
        
        # Determine if we should include prerelease
        $includePrerelease = Test-PreReleaseVersion -Version $currentVersion
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Include prerelease: $includePrerelease"
        }
        
        # Get major version if needed
        $majorVersion = ""
        if ($preserveMajor) {
            $majorVersion = $currentVersion.Split('.')[0]
            if ($EnableVerboseLogging) {
                Write-Host "  [VERBOSE] Preserving major version: $majorVersion"
            }
        }
        
        # Get latest version
        $latestVersion = Get-LatestPackageVersion -PackageId $packageId -IncludePrerelease $includePrerelease -MajorVersion $majorVersion
        
        if (-not $latestVersion) {
            Write-Host "No update available for '$packageId'"
            continue
        }
        
        # Compare versions
        $selectedVersion = Get-LatestVersionFromString -First $currentVersion -Second $latestVersion
        
        if ($EnableVerboseLogging) {
            Write-Host "  [VERBOSE] Version comparison: current=$currentVersion, latest=$latestVersion, selected=$selectedVersion"
        }
        
        if ($selectedVersion -ne $currentVersion) {
            Write-Host "##[section]Updating '$packageId' from '$currentVersion' to '$latestVersion'"
            $packageVersion.Version = $latestVersion
            $updateCount++
        } else {
            Write-Host "Package '$packageId' already at latest version '$currentVersion'"
        }
    }
}

if ($updateCount -gt 0) {
    Write-Host "##[section]Saving $updateCount package updates to $propsFile"
    $xml.Save($propsFile)
    Write-Host "Successfully updated $updateCount packages"
} else {
    Write-Host "No package updates needed"
}

Write-Host "==============================================================================="
Write-Host "NuGet Package Update Complete"
Write-Host "==============================================================================="
