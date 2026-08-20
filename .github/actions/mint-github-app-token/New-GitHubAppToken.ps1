# Mints a short-lived Microsoft Omex installation token using the App private
# key stored in Azure Key Vault. Shared by GitHub Actions and Azure Pipelines.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$apiUrl = 'https://api.github.com'
$outputVariable = 'GitHubAppToken'

$clientId = 'Iv23lid6KuWM6H8RIU1i'
$owner = 'microsoft'
$repositoryName = 'Omex'
$vaultName = 'OmexOpenSourceKV'
$secretName = 'microsoft-omex-github-app'

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

function Get-GitHubAppPrivateKey
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $VaultName,
        [Parameter(Mandatory = $true)]
        [string] $SecretName
    )

    $secretJson = az keyvault secret show --vault-name $VaultName --name $SecretName --query value --output json | Out-String
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($secretJson))
    {
        throw "Could not read secret '$SecretName' from Azure Key Vault '$VaultName'. Ensure the caller is signed in with an identity holding the 'Key Vault Secrets User' role."
    }

    $secret = $secretJson | ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace($secret))
    {
        throw "Azure Key Vault secret '$SecretName' is empty."
    }

    if ($secret.TrimStart().StartsWith('-----BEGIN'))
    {
        return $secret
    }

    try
    {
        $decoded = [System.Text.Encoding]::UTF8.GetString(
            [System.Convert]::FromBase64String(($secret -replace '\s', '')))
    }
    catch
    {
        throw "Azure Key Vault secret '$SecretName' must contain the GitHub App RSA private key as PEM text or Base64 encoded PEM. A GitHub App OAuth client secret cannot mint an installation token."
    }

    if (-not $decoded.TrimStart().StartsWith('-----BEGIN'))
    {
        throw "Azure Key Vault secret '$SecretName' must contain the GitHub App RSA private key as PEM text or Base64 encoded PEM. A GitHub App OAuth client secret cannot mint an installation token."
    }

    return $decoded
}

function Get-JsonWebToken
{
    param
    (
        [Parameter(Mandatory = $true)]
        [string] $ClientId,
        [Parameter(Mandatory = $true)]
        [string] $PrivateKey
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

    $rsa = [System.Security.Cryptography.RSA]::Create()
    try
    {
        try
        {
            $rsa.ImportFromPem($PrivateKey)
        }
        catch
        {
            throw "Azure Key Vault secret '$secretName' does not contain a valid RSA private key: $($_.Exception.Message)"
        }

        $signature = $rsa.SignData(
            [System.Text.Encoding]::ASCII.GetBytes($signingInput),
            [System.Security.Cryptography.HashAlgorithmName]::SHA256,
            [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
    }
    finally
    {
        $rsa.Dispose()
    }

    return "$signingInput.$(ConvertTo-Base64Url -Bytes $signature)"
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

$privateKey = Get-GitHubAppPrivateKey -VaultName $vaultName -SecretName $secretName
$jwt = Get-JsonWebToken -ClientId $clientId -PrivateKey $privateKey

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
    "token=$($accessToken.token)" | Out-File -FilePath $env:GITHUB_OUTPUT -Append
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
