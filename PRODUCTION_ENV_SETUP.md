# Production Environment Variables Setup (Render)

## ?? Render Dashboard Configuration

### Navigate to Your Service
1. Go to [Render Dashboard](https://dashboard.render.com)
2. Select your **MinimartApi** service
3. Go to **Environment** tab
4. Click **Add Environment Variable**

---

## ?? Required Environment Variables

### Copy and paste these into Render (update values with your actual credentials):

```bash
# ============================================
# DATABASE
# ============================================
ConnectionStrings__DefaultConnection=Host=ep-crimson-grass-a4i27e9q-pooler.us-east-1.aws.neon.tech;Database=Quickcrate_Staging3;Username=neondb_owner;Password=YOUR_NEW_PASSWORD;SSL Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;Connection Idle Lifetime=300;Command Timeout=30;Keepalive=60;Tcp Keepalive=true;Channel Binding=Require;

# ============================================
# REDIS
# ============================================
ConnectionStrings__Redis=your-production-redis-url:6379
REDIS_URL=rediss://your-upstash-url
REDIS_PASSWORD=your-redis-password

# ============================================
# JWT AUTHENTICATION
# ============================================
JwtSettings__Secret=YOUR_BASE64_JWT_SECRET_KEY
JwtSettings__Issuer=YourIssuer
JwtSettings__Audience=YourAudience
JwtSettings__ExpirationInMinutes=160
JwtSettings__RefreshTokenExpiryDays=7

# Legacy JWT (if still needed)
Jwt__key=YOUR_LEGACY_JWT_KEY

# ============================================
# M-PESA PRODUCTION
# ============================================
MpesaGoLive__ConsumerSecret=YOUR_MPESA_CONSUMER_SECRET
MpesaGoLive__ConsumerKey=YOUR_MPESA_CONSUMER_KEY
MpesaGoLive__ShortCode=YOUR_SHORTCODE
MpesaGoLive__Passkey=YOUR_MPESA_PASSKEY
MpesaGoLive__ConfirmationUrl=https://orderapi-33pp.onrender.com/api/Payment/confirmation
MpesaGoLive__ValidationUrl=https://orderapi-33pp.onrender.com/api/Payment/validation
MpesaGoLive__CallbackUrl=https://orderapi-33pp.onrender.com/api/Payment/stkcallback

# ============================================
# M-PESA SANDBOX (Optional)
# ============================================
MpesaSandBox__ConsumerSecret=YOUR_SANDBOX_SECRET
MpesaSandBox__ConsumerKey=YOUR_SANDBOX_KEY
MpesaSandBox__PassKey=YOUR_SANDBOX_PASSKEY

# ============================================
# EMAIL SERVICES
# ============================================
# Brevo
Brevo__SmtpUser=YOUR_BREVO_SMTP_USER
Brevo__SmtpPass=YOUR_BREVO_SMTP_PASSWORD
Brevo__SmtpHost=smtp-relay.brevo.com
Brevo__SmtpPort=587
Brevo__FromName=QuickCrate Express Limited
Brevo__FromAddress=noreply@quickcrate.co.ke

# Zoho (if used)
Zoho__SmtpUser=YOUR_ZOHO_EMAIL
Zoho__SmtpPass=YOUR_ZOHO_PASSWORD
Zoho__SmtpHost=smtp.zoho.com
Zoho__SmtpPort=587

# ============================================
# SMS SERVICE (CelcomAfrica)
# ============================================
CelcomAfrica__Apikey=YOUR_CELCOM_API_KEY
CelcomAfrica__PartnerID=YOUR_PARTNER_ID
CelcomAfrica__Shortcode=TEXTME
CelcomAfrica__url=https://isms.celcomafrica.com/api/services/sendsms/

# ============================================
# AWS
# ============================================
AWS__AccessKey=YOUR_AWS_ACCESS_KEY
AWS__SecretKey=YOUR_AWS_SECRET_KEY
AWS__Region=eu-north-1
AWS__Profile=default

# ============================================
# OPENSEARCH
# ============================================
OpenSearchSettings__Endpoint=https://your-opensearch-endpoint.eu-north-1.es.amazonaws.com

# ============================================
# RABBITMQ (Production)
# ============================================
RabbitMq__Uri=amqps://your-cloudamqp-url
RabbitMq__AutomaticRecoveryEnabled=true
RabbitMq__NetworkRecoveryInterval=00:00:10

# ============================================
# APPLICATION SETTINGS
# ============================================
Application__MerchantDashboardUrl=https://dashboard.quickcrate.co.ke
ASPNETCORE_ENVIRONMENT=Production
AllowedHosts=*

# ============================================
# REDIS SETTINGS (Production Overrides)
# ============================================
RedisSettings__UseSSL=true
RedisSettings__IsUpstash=true
RedisSettings__UpstashEndpoint=your-upstash-endpoint.upstash.io:6379
RedisSettings__ConnectTimeout=10000
RedisSettings__SyncTimeout=5000
RedisSettings__AbortOnConnectFail=false
```

---

## ?? Security Best Practices

### 1. **Use Render's Secret Management**
- Mark sensitive variables as **Secret** in Render dashboard
- This hides values in logs and UI

### 2. **Rotate All Exposed Credentials**
Before deploying to production, rotate:
- ? Database passwords (Neon DB)
- ? Redis passwords (Upstash)
- ? M-Pesa API keys (contact Safaricom)
- ? AWS access keys (AWS IAM Console)
- ? SMTP passwords (Brevo/Zoho dashboards)
- ? SMS API keys (CelcomAfrica)
- ? JWT secret (generate new Base64 key)

### 3. **Generate Secure JWT Secret**

```bash
# PowerShell - Generate new JWT secret
$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

```bash
# Linux/Mac - Generate new JWT secret
openssl rand -base64 32
```

---

## ?? Testing Environment Variables

### Test locally with Render environment:

Create `appsettings.Production.json` (DO NOT commit secrets):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Test with production environment:
```bash
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run
```

---

## ?? Render Environment Variable Formats

### Important Notes:
1. **Nested Configuration**: Use double underscores `__` to represent nested JSON
   ```bash
   ConnectionStrings__DefaultConnection   # Maps to ConnectionStrings:DefaultConnection
   JwtSettings__Secret                    # Maps to JwtSettings:Secret
   ```

2. **Case Sensitivity**: Environment variables are case-sensitive on Linux

3. **No Quotes**: Don't wrap values in quotes in Render dashboard

4. **Connection Strings**: Keep as single line (no line breaks)

---

## ? Verification Checklist

Before going live:

- [ ] All environment variables set in Render dashboard
- [ ] Sensitive variables marked as **Secret**
- [ ] Database connection tested (check Render logs)
- [ ] Redis connection working
- [ ] M-Pesa sandbox tested successfully
- [ ] Email sending works (Brevo/Zoho)
- [ ] JWT authentication tested
- [ ] AWS S3 uploads working
- [ ] OpenSearch queries working
- [ ] No secrets in Git repository
- [ ] All old credentials rotated

---

## ?? Debugging Production Issues

### View Environment Variables (in Render Shell):
```bash
env | grep ConnectionStrings
env | grep JwtSettings
env | grep AWS
```

### Check Configuration Loading:
Add temporary logging in `Program.cs`:
```csharp
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
Console.WriteLine($"JWT Secret loaded: {!string.IsNullOrEmpty(jwtSecret)}");
```

### Common Issues:
1. **"Connection string not found"**
   - Verify `ConnectionStrings__DefaultConnection` is set
   - Check for typos in variable name

2. **"JWT validation failed"**
   - Ensure `JwtSettings__Secret` matches between environments
   - Verify it's Base64 encoded

3. **"Redis connection failed"**
   - Check `REDIS_URL` format
   - Verify SSL is enabled for cloud Redis

---

## ?? Support

If you encounter issues:
1. Check Render deployment logs
2. Verify environment variables in Render dashboard
3. Test locally with production-like settings
4. Contact tech lead for credential verification

---

## ?? Additional Resources

- [Render Environment Variables](https://render.com/docs/environment-variables)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Neon Database Connection Pooling](https://neon.tech/docs/connect/connection-pooling)
