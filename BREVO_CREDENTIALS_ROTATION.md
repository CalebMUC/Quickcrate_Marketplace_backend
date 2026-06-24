# ?? URGENT: Update Brevo SMTP Credentials

## ?? Your Brevo Password Was Exposed

The password `O3ACj5rmPbgTH1VR` was hardcoded in `BrevoEmailService.cs` and committed to Git.

---

## ? Steps to Rotate Brevo Credentials

### Step 1: Generate New SMTP Password (5 minutes)

1. **Login to Brevo**:
   - Go to [https://app.brevo.com/](https://app.brevo.com/)
   - Login with your account

2. **Navigate to SMTP Settings**:
   - Click **SMTP & API** in the left sidebar
   - Go to **SMTP** tab

3. **Generate New Password**:
   - Click **"Generate a new password"** or **"Reset password"**
   - Copy the new password immediately (you won't see it again!)

4. **Verify SMTP User**:
   - Your SMTP username should be: `a1176e001@smtp-brevo.com`
   - If different, note it down

---

### Step 2: Update Local User Secrets (2 minutes)

```powershell
# Update Brevo credentials in User Secrets
dotnet user-secrets set "Brevo:SmtpUser" "a1176e001@smtp-brevo.com"
dotnet user-secrets set "Brevo:SmtpPass" "YOUR_NEW_BREVO_PASSWORD_HERE"

# Verify they're set
dotnet user-secrets list | Select-String "Brevo"
```

**Expected Output**:
```
Brevo:SmtpUser = a1176e001@smtp-brevo.com
Brevo:SmtpPass = YOUR_NEW_PASSWORD
Brevo:FromName = QuickCrate Express Limited
Brevo:FromAddress = noreply@quickcrate.co.ke
Brevo:SmtpHost = smtp-relay.brevo.com
Brevo:SmtpPort = 587
```

---

### Step 3: Update Production Environment Variables (Render) (3 minutes)

1. **Go to Render Dashboard**:
   - [https://dashboard.render.com](https://dashboard.render.com)
   - Select your `MinimartApi` service

2. **Update Environment Variables**:
   - Navigate to **Environment** tab
   - Update or add these variables:

```bash
Brevo__SmtpUser=a1176e001@smtp-brevo.com
Brevo__SmtpPass=YOUR_NEW_BREVO_PASSWORD
Brevo__FromName=QuickCrate Express Limited
Brevo__FromAddress=noreply@quickcrate.co.ke
Brevo__SmtpHost=smtp-relay.brevo.com
Brevo__SmtpPort=587
```

3. **Save Changes**:
   - Click **Save Changes**
   - Render will automatically redeploy

---

### Step 4: Test Email Sending (2 minutes)

#### Test Locally:

```powershell
# Run the application
dotnet run

# Test email endpoint (if you have one)
# Or create a test endpoint temporarily
```

#### Test in Production:

```bash
# Call your API endpoint that sends emails
curl -X POST https://orderapi-33pp.onrender.com/api/test-email \
  -H "Content-Type: application/json" \
  -d '{"email":"your-test-email@example.com"}'
```

---

### Step 5: Verify in Brevo Dashboard (2 minutes)

1. **Check Statistics**:
   - Brevo Dashboard ? **Statistics**
   - Verify you see recent sent emails

2. **Check Logs**:
   - Brevo Dashboard ? **Logs**
   - Verify delivery status

---

## ?? Troubleshooting

### "535 Authentication failed"
- ? **Cause**: Old password still being used
- ? **Fix**: Verify User Secrets and Environment Variables are updated
```powershell
dotnet user-secrets list | Select-String "Brevo"
```

### "SMTP credentials not configured"
- ? **Cause**: User Secrets not set
- ? **Fix**: Run the commands in Step 2 above

### Email not appearing in Brevo dashboard
- ? **Cause**: Wrong SMTP user or authentication failed
- ? **Fix**: Verify SMTP username in Brevo dashboard matches User Secrets

---

## ?? Checklist

Before proceeding, ensure:

- [ ] Generated new SMTP password in Brevo dashboard
- [ ] Updated local User Secrets with new password
- [ ] Verified User Secrets with `dotnet user-secrets list`
- [ ] Updated Render environment variables
- [ ] Tested email sending locally
- [ ] Tested email sending in production
- [ ] Verified in Brevo dashboard that emails are being sent
- [ ] Deleted old hardcoded password from any documentation

---

## ?? Security Reminder

### ? Going Forward

- **NEVER** hardcode credentials in source code
- **ALWAYS** use User Secrets for local development
- **ALWAYS** use Environment Variables for production
- **ROTATE** credentials immediately if exposed
- **CHECK** Git history for exposed secrets

### ?? If You See This Pattern

```csharp
// ? BAD - Hardcoded credentials
await client.AuthenticateAsync("user@smtp.com", "password123");

// ? GOOD - Configuration-based
var user = _configuration["Brevo:SmtpUser"];
var pass = _configuration["Brevo:SmtpPass"];
await client.AuthenticateAsync(user, pass);
```

**Report it immediately and rotate credentials!**

---

## ?? Quick Reference

### Brevo SMTP Settings

| Setting | Value |
|---------|-------|
| Host | smtp-relay.brevo.com |
| Port | 587 |
| Encryption | STARTTLS |
| User | a1176e001@smtp-brevo.com |
| Password | (Get from Brevo dashboard) |

### User Secrets Commands

```bash
# Set a secret
dotnet user-secrets set "Key:SubKey" "Value"

# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "Key:SubKey"

# Clear all secrets
dotnet user-secrets clear
```

---

## ?? Estimated Time

| Step | Time |
|------|------|
| 1. Generate new password | 5 min |
| 2. Update User Secrets | 2 min |
| 3. Update Render | 3 min |
| 4. Test | 2 min |
| 5. Verify | 2 min |
| **Total** | **~15 minutes** |

---

**Complete these steps ASAP to secure your email service!** ??
