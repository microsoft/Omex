<#

.SYNOPSIS

This script validates NuGet packages for required attributes.

.EXAMPLE

Microsoft.Omex.Tools.ValidateNuGetPackages.ps1 c:\omex\nuget

.DESCRIPTION

There are several required attributes in NuGet specification files (nuspec) which are required by Microsoft
when the NuGet packages are published to NuGet.org.

If these attributes are missing, the packages cannot be published.

.NOTES

The script is intended to be used as a step in the pull request validation pipeline.
The script exits with code 1 if the validation failed.

The script verifies if the following attributes are present in the nuspec file:
 - title
 - description
 - project url
 - tags


.PARAMETER NuGetFolder
Path which contains the NuGet packages to be scanned.
The path is scanned recursively for *.nupkg files.

#>


param (
	[Parameter(Mandatory = $true)]
	[string]$NuGetFolder
)

<#
The validates the NuGet package for the required attributes
and returns false if some of them is missing.
#>
function ValidateNuGetPackage([string]$nugetPackage) {
    Write-Host "Validating $nugetPackage"

    $contents = ExtractNuSpec($nugetPackage)
    $xmldoc = [xml]$contents

    $result = $True

    if ($xmldoc.package.metadata.title.Length -eq 0)
    {
        Write-Error "Title is missing."
        $result = $False
    }

    if ($xmldoc.package.metadata.description.Length -eq 0)
    {
        Write-Error "Description is missing."
        $result = $False
    }

    if ($xmldoc.package.metadata.projecturl.Length -eq 0)
    {
        Write-Error "ProjectUrl is missing."
        $result = $False
    }

    if ($xmldoc.package.metadata.tags.Length -eq 0)
    {
        Write-Error "Tags is missing."
        $result = $False
    }

    if ($result -eq $True)
    {
        Write-Host "Successfully validated NuGet package $nugetPackage"
    }
    else
    {
        Write-Error "NuGet package validation failed for $nugetPackage"
    }

    return $result
}

<#
The function extracts the contents of the NuGet specification file
from the NuGet archive.
#>
function ExtractNuSpec([string]$nugetPackage)
{
    $zip = [io.compression.zipfile]::OpenRead($nugetPackage)
    $file = $null
    foreach ($entry in $zip.Entries)
    {
        if ($entry.FullName.EndsWith('.nuspec'))
        {
            $file = $entry
            break
        }
    }

    if ($null -eq $file)
    {
        Write-Error "Unable to find NuGet specification in NuGet package $nugetPackage"
        return $false
    }

    $stream = $file.Open()
    $reader = New-Object IO.StreamReader($stream)
    $text = $reader.ReadToEnd()

    $reader.Close()
    $stream.Close()
    $zip.Dispose()

    return $text
}

Add-Type -assembly "system.io.compression.filesystem"

Write-Host "Scanning folder $NuGetFolder"

#Scanning the input folder recursively to get a list of NuGet packages to validate
$nugetFiles = Get-ChildItem "$NuGetFolder\\*.nupkg" -Recurse

Write-Host "The following $($nugetFiles.Count) NuGet packages will be validated"
foreach ($file in $nugetFiles)
{
    Write-Host $file
}

$result = $true
foreach ($file in $nugetFiles)
{
    $currentResult = ValidateNuGetPackage $file
    if ($currentResult -eq $false)
    {
        $result = $false
    }
}

if ($result -eq $false)
{
    exit 1
}
