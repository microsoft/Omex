# Mints a short-lived Microsoft Omex installation token by signing the App JWT
# remotely in Azure Key Vault. Shared by GitHub Actions and Azure Pipelines.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$apiUrl = 'https://api.github.com'
$keyVaultApiVersion = '2025-07-01'
$outputVariable = 'GitHubAppToken'

# GitHub App: Microsoft Omex
$clientId = 'Iv23lid6KuWM6H8RIU1i'
$owner = 'microsoft'
$repositoryName = 'Omex'
$vaultName = 'OmexOpenSourceKV'
$keyName = 'microsoft-omex-github-app'

$permissionsJson = $env:GITHUB_APP_PERMISSIONS
if ([string]::IsNullOrWhiteSpace($permissionsJson))
{
    throw "The 'GITHUB_APP_PERMISSIONS' environment variable is not set."
}

function ConvertTo-Base64Url
{
    param
    (
        [Parameter(Mandatory = $true)]
        [byte[]] $Bytes
    )

    return [System.Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
}

function Get-KeyVaultAccessToken
{
    $token = az account get-access-token --resource 'https://vault.azure.net' --query accessToken --output tsv
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($token))
    {
        throw 'Could not acquire an Azure Key Vault access token. Ensure the caller is signed in with an identity holding the ''Key Vault Crypto User'' role on the vault.'
    }

    return $token.Trim()
}

function Get-JsonWebToken
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ClientId,
        [Parameter(Mandatory = $true)]
        [string] $VaultName,
        [Parameter(Mandatory = $true)]
        [string] $KeyName,
        [Parameter(Mandatory = $true)]
        [string] $VaultAccessToken
    )

    $issuedAt = [System.DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $header = [ordered] @{
        alg = 'RS256'
        typ = 'JWT'
    }
    $payload = [ordered] @{
        exp = $issuedAt + 540
        iat = $issuedAt - 60
        iss = $ClientId
    }

    $headerEncoded = ConvertTo-Base64Url -Bytes ([System.Text.Encoding]::UTF8.GetBytes(($header | ConvertTo-Json -Compress)))
    $payloadEncoded = ConvertTo-Base64Url -Bytes ([System.Text.Encoding]::UTF8.GetBytes(($payload | ConvertTo-Json -Compress)))
    $signingInput = "$headerEncoded.$payloadEncoded"

    $digest = [System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::ASCII.GetBytes($signingInput))
    $signUri = "https://$VaultName.vault.azure.net/keys/$KeyName/sign?api-version=$keyVaultApiVersion"
    $signBody = @{
        alg   = 'RS256'
        value = ConvertTo-Base64Url -Bytes $digest
    }

    try
    {
        $response = Invoke-RestMethod -Uri $signUri -Method 'Post' -ContentType 'application/json' -Body ($signBody | ConvertTo-Json -Compress) -Headers @{
            Authorization = "Bearer $VaultAccessToken"
        }
    }
    catch
    {
        $detail = $_.ErrorDetails.Message
        if ([string]::IsNullOrWhiteSpace($detail))
        {
            $detail = $_.Exception.Message
        }

        throw "The Azure Key Vault sign request to '$signUri' failed: $detail"
    }

    if ([string]::IsNullOrWhiteSpace($response.value))
    {
        throw 'The Azure Key Vault sign request returned no signature.'
    }

    return "$signingInput.$($response.value)"
}

function Invoke-GitHubApi
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $Uri,
        [Parameter(Mandatory = $true)]
        [string] $Jwt,
        [Parameter(Mandatory = $true)]
        [string] $Method,
        [Parameter(Mandatory = $false)]
        [object] $Body
    )

    $parameters = @{
        Uri       = $Uri
        Method    = $Method
        UserAgent = 'Omex'
        Headers   = @{
            Accept                 = 'application/vnd.github+json'
            # Review tooling may redact this value in diffs. Runtime uses the supplied JWT.
            Authorization          = "Bearer $Jwt"
            'X-GitHub-Api-Version' = '2022-11-28'
        }
    }
    if ($null -ne $Body)
    {
        $parameters.Body = $Body | ConvertTo-Json -Compress -Depth 10
        $parameters.ContentType = 'application/json'
    }

    try
    {
        return Invoke-RestMethod @parameters
    }
    catch
    {
        $detail = $_.ErrorDetails.Message
        if ([string]::IsNullOrWhiteSpace($detail))
        {
            $detail = $_.Exception.Message
        }

        throw "GitHub API request to '$Uri' failed: $detail"
    }
}

$vaultAccessToken = Get-KeyVaultAccessToken
if ($env:GITHUB_ACTIONS -eq 'true')
{
    Write-Output -InputObject "::add-mask::$vaultAccessToken"
}
elseif (-not [string]::IsNullOrWhiteSpace($env:TF_BUILD))
{
    Write-Output -InputObject "##vso[task.setvariable variable=KeyVaultAccessToken;issecret=true]$vaultAccessToken"
}

$jwt = Get-JsonWebToken -ClientId $clientId -VaultName $vaultName -KeyName $keyName -VaultAccessToken $vaultAccessToken

$installation = Invoke-GitHubApi -Uri "$apiUrl/repos/$owner/$repositoryName/installation" -Jwt $jwt -Method 'Get'
if ($null -eq $installation.id)
{
    throw "Could not determine the App installation for '$owner/$repositoryName'."
}

$body = @{
    repositories = @($repositoryName)
    permissions  = ($permissionsJson | ConvertFrom-Json)
}
$accessToken = Invoke-GitHubApi -Uri "$apiUrl/app/installations/$($installation.id)/access_tokens" -Jwt $jwt -Method 'Post' -Body $body
if ([string]::IsNullOrWhiteSpace($accessToken.token))
{
    throw 'The GitHub App installation token could not be minted.'
}

if ($env:GITHUB_ACTIONS -eq 'true')
{
    Write-Output -InputObject "::add-mask::$($accessToken.token)"
    "token=$($accessToken.token)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
}
elseif (-not [string]::IsNullOrWhiteSpace($env:TF_BUILD))
{
    Write-Output -InputObject "##vso[task.setvariable variable=$outputVariable;issecret=true]$($accessToken.token)"
}
else
{
    throw 'Unable to determine the CI host: neither GITHUB_ACTIONS nor TF_BUILD is set.'
}

Write-Output -InputObject "Minted an installation token for '$owner/$repositoryName' (installation $($installation.id))."
