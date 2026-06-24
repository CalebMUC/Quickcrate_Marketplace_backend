# PowerShell Script to Configure User Secrets for Local Development
# Run this script from the project directory

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  QuickCrate API - User Secrets Setup  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verify we're in the correct directory
if (-not (Test-Path "MinimartApi.csproj")) {
    Write-Host "? Error: MinimartApi.csproj not found!" -ForegroundColor Red
    Write-Host "Please run this script from the project root directory." -ForegroundColor Yellow
    exit 1
}

Write-Host "? Found MinimartApi.csproj" -ForegroundColor Green
Write-Host ""

# Prompt user for secrets
Write-Host "Please provide the following secrets:" -ForegroundColor Yellow
Write-Host "(Press Enter to skip any optional values)" -ForegroundColor Gray
Write-Host ""

# Database Connection
Write-Host "?? Database Configuration" -ForegroundColor Cyan
$dbConnection = Read-Host "PostgreSQL Connection String"

# JWT Settings
Write-Host ""
Write-Host "?? JWT Configuration" -ForegroundColor Cyan
$jwtSecret = Read-Host "JWT Secret Key (Base64)"

# M-Pesa Production
Write-Host ""
Write-Host "?? M-Pesa Production" -ForegroundColor Cyan
$mpesaConsumerKey = Read-Host "M-Pesa Consumer Key"
$mpesaConsumerSecret = Read-Host "M-Pesa Consumer Secret"
$mpesaShortCode = Read-Host "M-Pesa ShortCode"
$mpesaPasskey = Read-Host "M-Pesa Passkey"

# M-Pesa Sandbox
Write-Host ""
Write-Host "?? M-Pesa Sandbox (Optional)" -ForegroundColor Cyan
$sandboxConsumerKey = Read-Host "Sandbox Consumer Key (optional)"
$sandboxConsumerSecret = Read-Host "Sandbox Consumer Secret (optional)"
$sandboxPasskey = Read-Host "Sandbox Passkey (optional)"

# Email Services
Write-Host ""
Write-Host "?? Email Services" -ForegroundColor Cyan
$brevoUser = Read-Host "Brevo SMTP User"
$brevoPass = Read-Host "Brevo SMTP Password"

# AWS
Write-Host ""
Write-Host "?? AWS Configuration" -ForegroundColor Cyan
$awsAccessKey = Read-Host "AWS Access Key"
$awsSecretKey = Read-Host "AWS Secret Key"

# OpenSearch
Write-Host ""
Write-Host "?? OpenSearch Configuration" -ForegroundColor Cyan
$openSearchEndpoint = Read-Host "OpenSearch Endpoint"

# SMS Service
Write-Host ""
Write-Host "?? SMS Service (CelcomAfrica)" -ForegroundColor Cyan
$celcomApiKey = Read-Host "Celcom API Key (optional)"
$celcomPartnerId = Read-Host "Celcom Partner ID (optional)"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Setting User Secrets..." -ForegroundColor Yellow
Write-Host ""

# Set required secrets
if ($dbConnection) {
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $dbConnection
    Write-Host "? Database connection configured" -ForegroundColor Green
}

if ($jwtSecret) {
    dotnet user-secrets set "JwtSettings:Secret" $jwtSecret
    Write-Host "? JWT secret configured" -ForegroundColor Green
}

if ($mpesaConsumerKey) {
    dotnet user-secrets set "MpesaGoLive:ConsumerKey" $mpesaConsumerKey
    Write-Host "? M-Pesa Consumer Key configured" -ForegroundColor Green
}

if ($mpesaConsumerSecret) {
    dotnet user-secrets set "MpesaGoLive:ConsumerSecret" $mpesaConsumerSecret
    Write-Host "? M-Pesa Consumer Secret configured" -ForegroundColor Green
}

if ($mpesaShortCode) {
    dotnet user-secrets set "MpesaGoLive:ShortCode" $mpesaShortCode
    Write-Host "? M-Pesa ShortCode configured" -ForegroundColor Green
}

if ($mpesaPasskey) {
    dotnet user-secrets set "MpesaGoLive:Passkey" $mpesaPasskey
    Write-Host "? M-Pesa Passkey configured" -ForegroundColor Green
}

# Sandbox (optional)
if ($sandboxConsumerKey) {
    dotnet user-secrets set "MpesaSandBox:ConsumerKey" $sandboxConsumerKey
}
if ($sandboxConsumerSecret) {
    dotnet user-secrets set "MpesaSandBox:ConsumerSecret" $sandboxConsumerSecret
}
if ($sandboxPasskey) {
    dotnet user-secrets set "MpesaSandBox:PassKey" $sandboxPasskey
}

if ($brevoUser) {
    dotnet user-secrets set "Brevo:SmtpUser" $brevoUser
    Write-Host "? Brevo SMTP User configured" -ForegroundColor Green
}

if ($brevoPass) {
    dotnet user-secrets set "Brevo:SmtpPass" $brevoPass
    Write-Host "? Brevo SMTP Password configured" -ForegroundColor Green
}

if ($awsAccessKey) {
    dotnet user-secrets set "AWS:AccessKey" $awsAccessKey
    Write-Host "? AWS Access Key configured" -ForegroundColor Green
}

if ($awsSecretKey) {
    dotnet user-secrets set "AWS:SecretKey" $awsSecretKey
    Write-Host "? AWS Secret Key configured" -ForegroundColor Green
}

if ($openSearchEndpoint) {
    dotnet user-secrets set "OpenSearchSettings:Endpoint" $openSearchEndpoint
    Write-Host "? OpenSearch Endpoint configured" -ForegroundColor Green
}

if ($celcomApiKey) {
    dotnet user-secrets set "CelcomAfrica:Apikey" $celcomApiKey
}

if ($celcomPartnerId) {
    dotnet user-secrets set "CelcomAfrica:PartnerID" $celcomPartnerId
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? User Secrets Setup Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To view all secrets, run:" -ForegroundColor Yellow
Write-Host "  dotnet user-secrets list" -ForegroundColor White
Write-Host ""
Write-Host "To remove a secret, run:" -ForegroundColor Yellow
Write-Host "  dotnet user-secrets remove 'SecretKey'" -ForegroundColor White
Write-Host ""
Write-Host "Secrets are stored at:" -ForegroundColor Yellow
Write-Host "  %APPDATA%\Microsoft\UserSecrets\95e4bf66-ce97-40ba-9993-4a85a220bc9d\" -ForegroundColor White
Write-Host ""
