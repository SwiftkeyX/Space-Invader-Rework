# Generates a GitHub App installation access token and configures gh CLI.
# Run this script whenever the token expires (~1 hour).

# Two levels up from .claude/scripts/ → project root
$projectRoot = (Resolve-Path "$PSScriptRoot\..\..")
$envFile = Join-Path $projectRoot ".env"

$envVars = @{}
Get-Content $envFile | ForEach-Object {
    if ($_ -match "^([^=]+)=(.*)$") { $envVars[$matches[1]] = $matches[2] }
}

$appId          = $envVars["GITHUB_APP_ID"]
$installationId = $envVars["GITHUB_APP_INSTALLATION_ID"]
$keyPath        = $envVars["GITHUB_APP_PRIVATE_KEY_PATH"]

# Try with .pem extension if bare path doesn't exist
if (-not (Test-Path $keyPath)) { $keyPath = "$keyPath.pem" }
if (-not (Test-Path $keyPath)) { Write-Error "Private key not found at: $keyPath"; exit 1 }

$pem = Get-Content $keyPath -Raw
$b64 = $pem -replace "-----[^-]+-----|[\r\n\s]", ""
$keyBytes = [Convert]::FromBase64String($b64)

$rsa = [System.Security.Cryptography.RSA]::Create()
try {
    $rsa.ImportRSAPrivateKey($keyBytes, [ref]$null)  # PKCS#1
} catch {
    $rsa.ImportPkcs8PrivateKey($keyBytes, [ref]$null) # PKCS#8 fallback
}

function ConvertTo-Base64Url($bytes) {
    [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+','-').Replace('/','_')
}

$now      = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$header   = '{"alg":"RS256","typ":"JWT"}'
$payload  = "{`"iat`":$($now - 60),`"exp`":$($now + 600),`"iss`":`"$appId`"}"

$h = ConvertTo-Base64Url([Text.Encoding]::UTF8.GetBytes($header))
$p = ConvertTo-Base64Url([Text.Encoding]::UTF8.GetBytes($payload))

$sig = $rsa.SignData(
    [Text.Encoding]::UTF8.GetBytes("$h.$p"),
    [Security.Cryptography.HashAlgorithmName]::SHA256,
    [Security.Cryptography.RSASignaturePadding]::Pkcs1
)
$jwt = "$h.$p.$(ConvertTo-Base64Url $sig)"

try {
    $response = Invoke-RestMethod `
        -Uri "https://api.github.com/app/installations/$installationId/access_tokens" `
        -Method POST `
        -Headers @{
            "Authorization"        = "Bearer $jwt"
            "Accept"               = "application/vnd.github+json"
            "X-GitHub-Api-Version" = "2022-11-28"
        } `
        -ErrorAction Stop
} catch {
    Write-Error "API call failed: $_"
    exit 1
}

if (-not $response.token) { Write-Error "No token in response: $($response | ConvertTo-Json)"; exit 1 }

$env:GH_TOKEN = $response.token
[System.Environment]::SetEnvironmentVariable("GH_TOKEN", $response.token, "User")

# Write token to project root so skills can read it with `cat .gh-token`
[System.IO.File]::WriteAllText((Join-Path $projectRoot ".gh-token"), $response.token)

Write-Host "Done. Token expires: $($response.expires_at)"
Write-Host "Token preview: $($response.token.Substring(0, 10))..."
