# Secret Management Guide

## ?? Overview

This project uses **User Secrets** for local development and **Environment Variables** for production deployment. Secrets are **never** committed to Git.

---

## ?? Initial Setup for Developers

### 1. **Verify User Secrets are Initialized**

Your project already has User Secrets configured with ID: `95e4bf66-ce97-40ba-9993-4a85a220bc9d`

### 2. **Configure Local Secrets**

#### Option A: Using Visual Studio
1. Right-click on the `MinimartApi` project
2. Select **Manage User Secrets**
3. Paste your secrets (see template below)

#### Option B: Using .NET CLI

```bash
# Navigate to project directory
cd E:\MinimartApi_Staging\OrderApi

# Add secrets one by one
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=your-host;Database=your-db;Username=user;Password=pass;..."
dotnet user-secrets set "JwtSettings:Secret" "your-base64-secret-key"
dotnet user-secrets set "AWS:AccessKey" "YOUR_AWS_KEY"
dotnet user-secrets set "AWS:SecretKey" "YOUR_AWS_SECRET"

# Or list existing secrets
dotnet user-secrets list
```

### 3. **User Secrets Template** (secrets.json)

Copy this template and fill in your actual values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=ep-crimson-grass-a4i27e9q-pooler.us-east-1.aws.neon.tech;Database=Quickcrate_Staging3;Username=neondb_owner;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;Connection Idle Lifetime=300;Command Timeout=30;Keepalive=60;Tcp Keepalive=true;Channel Binding=Require;",
    "Redis": "localhost:6379"
  },

  "JwtSettings": {
    "Secret": "YOUR_JWT_SECRET_BASE64",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience"
  },

  "MpesaGoLive": {
    "ConsumerSecret": "YOUR_MPESA_CONSUMER_SECRET",
    "ConsumerKey": "YOUR_MPESA_CONSUMER_KEY",
    "ShortCode": "YOUR_SHORTCODE",
    "Passkey": "YOUR_MPESA_PASSKEY"
  },

  "MpesaSandBox": {
    "ConsumerSecret": "YOUR_SANDBOX_SECRET",
    "ConsumerKey": "YOUR_SANDBOX_KEY",
    "PassKey": "YOUR_SANDBOX_PASSKEY"
  },

  "Brevo": {
    "SmtpUser": "YOUR_BREVO_USER",
    "SmtpPass": "YOUR_BREVO_PASSWORD"
  },

  "Zoho": {
    "SmtpUser": "YOUR_ZOHO_EMAIL",
    "SmtpPass": "YOUR_ZOHO_PASSWORD"
  },

  "CelcomAfrica": {
    "Apikey": "YOUR_CELCOM_API_KEY",
    "PartnerID": "YOUR_PARTNER_ID"
  },

  "AWS": {
    "AccessKey": "YOUR_AWS_ACCESS_KEY",
    "SecretKey": "YOUR_AWS_SECRET_KEY"
  },

  "OpenSearchSettings": {
    "Endpoint": "https://your-opensearch-endpoint.amazonaws.com"
  }
}
```

---

## ?? Production Deployment (Render/Cloud)

### Environment Variables Format

Set these in your Render dashboard or hosting platform:

```bash
# Database
ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=..."
ConnectionStrings__Redis="your-redis-connection"

# JWT
JwtSettings__Secret="your-jwt-secret"
JwtSettings__Issuer="YourIssuer"
JwtSettings__Audience="YourAudience"

# M-Pesa Production
MpesaGoLive__ConsumerSecret="your-secret"
MpesaGoLive__ConsumerKey="your-key"
MpesaGoLive__ShortCode="your-shortcode"
MpesaGoLive__Passkey="your-passkey"

# M-Pesa Sandbox
MpesaSandBox__ConsumerSecret="your-sandbox-secret"
MpesaSandBox__ConsumerKey="your-sandbox-key"
MpesaSandBox__PassKey="your-sandbox-passkey"

# Email Services
Brevo__SmtpUser="your-brevo-user"
Brevo__SmtpPass="your-brevo-password"
Zoho__SmtpUser="your-zoho-email"
Zoho__SmtpPass="your-zoho-password"

# SMS
CelcomAfrica__Apikey="your-celcom-key"
CelcomAfrica__PartnerID="your-partner-id"

# AWS
AWS__AccessKey="your-aws-key"
AWS__SecretKey="your-aws-secret"

# OpenSearch
OpenSearchSettings__Endpoint="your-opensearch-endpoint"

# Redis (Production)
REDIS_URL="your-redis-url"
REDIS_PASSWORD="your-redis-password"

# RabbitMQ (Production)
RabbitMq__Uri="your-rabbitmq-uri"
```

---

## ?? File Structure

```
??? appsettings.json                  ? Safe defaults only (committed)
??? appsettings.Template.json         ? Template for developers (committed)
??? appsettings.Development.json      ? Dev overrides (committed, no secrets)
??? appsettings.Production.json       ? Prod overrides (committed, no secrets)
??? User Secrets (secrets.json)       ?? Developer secrets (NOT committed)
    Location: %APPDATA%\Microsoft\UserSecrets\95e4bf66-ce97-40ba-9993-4a85a220bc9d\
```

---

## ?? Security Checklist

- [ ] All secrets removed from `appsettings.json`
- [ ] `appsettings.json` added to `.gitignore`
- [ ] User Secrets configured locally
- [ ] Environment Variables set in production (Render/Cloud)
- [ ] **ROTATE ALL EXPOSED CREDENTIALS** (database passwords, API keys, AWS keys, M-Pesa secrets)
- [ ] Team members have access to secure credential storage (password manager, Azure Key Vault, etc.)

---

## ?? How Configuration Overrides Work

ASP.NET Core loads configuration in this order (later sources override earlier):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific)
3. **User Secrets** (development only)
4. **Environment Variables** (all environments)
5. Command-line arguments

---

## ?? Troubleshooting

### "Configuration value is missing"
- **Local Dev**: Verify secret is set via `dotnet user-secrets list`
- **Production**: Check environment variables in hosting platform dashboard

### "User Secrets not loading"
- Ensure `<UserSecretsId>` exists in `MinimartApi.csproj` (already configured: `95e4bf66-ce97-40ba-9993-4a85a220bc9d`)
- User Secrets only work in **Development** environment

### "Still seeing old secrets"
- Clear build artifacts: Delete `bin/` and `obj/` folders
- Restart Visual Studio or your IDE

---

## ?? Additional Resources

- [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment Variables in .NET](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables)

---

## ?? Team Onboarding

New developers should:

1. Clone the repository
2. Copy `appsettings.Template.json` content
3. Configure User Secrets (Visual Studio or CLI)
4. Contact tech lead for actual credential values
5. **Never** commit `appsettings.json` with real secrets
