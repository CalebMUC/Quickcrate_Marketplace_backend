# ? Secret Management Implementation Checklist

## ?? Immediate Actions (Do Now)

### Phase 1: Local Migration (5-10 minutes)
- [ ] Run `.\migrate-secrets-ONCE.ps1` to migrate secrets to User Secrets
- [ ] Verify secrets loaded: `dotnet user-secrets list`
- [ ] Test application: `dotnet run`
- [ ] Confirm app works correctly (test login, database, etc.)
- [ ] Delete migration script: `Remove-Item migrate-secrets-ONCE.ps1`

### Phase 2: Secure Git Repository (30 minutes - CRITICAL)
- [ ] Verify `.gitignore` updated (already done ?)
- [ ] Stage cleaned `appsettings.json`: `git add appsettings.json`
- [ ] Commit changes: `git commit -m "Remove secrets from appsettings.json"`
- [ ] **CRITICAL**: Remove secrets from Git history (see options below)

#### Option 1: BFG Repo-Cleaner (Fastest)
```bash
# Download BFG from: https://rtyley.github.io/bfg-repo-cleaner/
java -jar bfg.jar --delete-files appsettings.json --no-blob-protection
git reflog expire --expire=now --all
git gc --prune=now --aggressive
git push --force
```

#### Option 2: git filter-repo (Recommended by GitHub)
```bash
# Install: pip install git-filter-repo
git filter-repo --path appsettings.json --invert-paths
git push --force --all
```

#### Option 3: Make Repository Private
- [ ] Go to GitHub Settings ? Danger Zone ? Change visibility
- [ ] Make repository **Private** to prevent further exposure

---

## ?? Phase 3: Rotate ALL Credentials (1-2 hours - CRITICAL)

### Database - Neon PostgreSQL
- [ ] Login to [Neon Console](https://console.neon.tech/)
- [ ] Navigate to your database: `Quickcrate_Staging3`
- [ ] Reset password for user `neondb_owner`
- [ ] Update User Secrets locally:
  ```bash
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Password=NEW_PASSWORD;..."
  ```
- [ ] Test connection: `dotnet run`

### M-Pesa Production Keys
- [ ] Contact Safaricom M-Pesa support or login to M-Pesa Portal
- [ ] Request new Consumer Key and Consumer Secret
- [ ] Generate new Passkey
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "MpesaGoLive:ConsumerKey" "NEW_KEY"
  dotnet user-secrets set "MpesaGoLive:ConsumerSecret" "NEW_SECRET"
  dotnet user-secrets set "MpesaGoLive:Passkey" "NEW_PASSKEY"
  ```
- [ ] Test M-Pesa STK Push

### M-Pesa Sandbox Keys
- [ ] Login to [M-Pesa Sandbox](https://developer.safaricom.co.ke/)
- [ ] Regenerate sandbox credentials
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "MpesaSandBox:ConsumerKey" "NEW_KEY"
  dotnet user-secrets set "MpesaSandBox:ConsumerSecret" "NEW_SECRET"
  dotnet user-secrets set "MpesaSandBox:PassKey" "NEW_PASSKEY"
  ```

### AWS Access Keys
- [ ] Login to [AWS Console](https://console.aws.amazon.com/iam/)
- [ ] Navigate to IAM ? Users ? Your User ? Security Credentials
- [ ] **Deactivate** old key: `AKIARJHKLYVKSAVJHAPL`
- [ ] Create new Access Key
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "AWS:AccessKey" "NEW_ACCESS_KEY"
  dotnet user-secrets set "AWS:SecretKey" "NEW_SECRET_KEY"
  ```
- [ ] Test S3 uploads

### Brevo Email Service
- [ ] Login to [Brevo Dashboard](https://app.brevo.com/)
- [ ] Navigate to SMTP & API ? SMTP
- [ ] Reset SMTP password
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "Brevo:SmtpPass" "NEW_PASSWORD"
  ```
- [ ] Test email sending

### Zoho Email Service
- [ ] Login to [Zoho Account](https://accounts.zoho.com/)
- [ ] Navigate to Security ? App Passwords
- [ ] Revoke old password, create new one
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "Zoho:SmtpPass" "NEW_PASSWORD"
  ```

### CelcomAfrica SMS Service
- [ ] Contact CelcomAfrica support
- [ ] Request API key rotation
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "CelcomAfrica:Apikey" "NEW_API_KEY"
  ```

### JWT Secret (Will invalidate all tokens)
- [ ] Generate new secret:
  ```powershell
  $bytes = New-Object byte[] 32
  [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
  [Convert]::ToBase64String($bytes)
  ```
- [ ] Update User Secrets:
  ```bash
  dotnet user-secrets set "JwtSettings:Secret" "NEW_BASE64_SECRET"
  ```
- [ ] **Note**: All users will need to re-login

---

## ?? Phase 4: Production Deployment (30 minutes)

### Configure Render Environment Variables
- [ ] Login to [Render Dashboard](https://dashboard.render.com)
- [ ] Select `MinimartApi` service
- [ ] Go to **Environment** tab
- [ ] Add ALL variables from `PRODUCTION_ENV_SETUP.md` (use NEW rotated credentials)
- [ ] Mark sensitive variables as **Secret**
- [ ] Save changes

### Key Variables to Set (with NEW credentials):
```bash
ConnectionStrings__DefaultConnection=NEW_VALUE
JwtSettings__Secret=NEW_VALUE
MpesaGoLive__ConsumerKey=NEW_VALUE
MpesaGoLive__ConsumerSecret=NEW_VALUE
AWS__AccessKey=NEW_VALUE
AWS__SecretKey=NEW_VALUE
Brevo__SmtpPass=NEW_VALUE
# ... etc
```

### Deploy and Test
- [ ] Deploy to Render (should auto-deploy on environment variable change)
- [ ] Check deployment logs for errors
- [ ] Test production API endpoints
- [ ] Verify database connectivity
- [ ] Test M-Pesa integration
- [ ] Test email sending
- [ ] Test AWS uploads

---

## ?? Phase 5: Team Communication (15 minutes)

### Update Team Members
- [ ] Send email/message to team with:
  - Link to `SECRET_MANAGEMENT_GUIDE.md`
  - Link to `QUICK_START_SECRETS.md`
  - Instructions to run `setup-user-secrets.ps1` for new setup
  - Reminder: NEVER commit secrets to Git

### Share Documentation
- [ ] `SECRET_MANAGEMENT_GUIDE.md` - Full guide
- [ ] `QUICK_START_SECRETS.md` - Quick reference
- [ ] `PRODUCTION_ENV_SETUP.md` - For DevOps team

---

## ? Verification & Testing

### Local Development
- [ ] Secrets loaded: `dotnet user-secrets list` shows all secrets
- [ ] App starts: `dotnet run` works without errors
- [ ] Database works: Can query/insert data
- [ ] JWT works: Can login and access protected endpoints
- [ ] Email works: Can send test emails
- [ ] M-Pesa works: Can initiate test payment
- [ ] AWS works: Can upload files to S3

### Production (Render)
- [ ] Deployment successful: No errors in Render logs
- [ ] Environment variables set: Visible in Render Environment tab
- [ ] Database connection: App connects to production DB
- [ ] API responds: Test endpoints return expected results
- [ ] Authentication works: JWT login functional
- [ ] Payments work: M-Pesa integration functional
- [ ] Emails send: Brevo/Zoho sending works

---

## ?? Security Verification

### Git Security
- [ ] `appsettings.json` contains NO secrets
- [ ] `git log -p appsettings.json` shows no secrets in history (if cleaned)
- [ ] `.gitignore` includes `appsettings.json`
- [ ] No `.env` or `secrets.json` files committed

### Secret Storage
- [ ] Local secrets in User Secrets (not in project folder)
- [ ] Production secrets in Render Environment Variables
- [ ] No secrets in any committed files
- [ ] Template files contain only placeholders

### Access Control
- [ ] Team members know how to set up User Secrets
- [ ] Production environment variables restricted to authorized users
- [ ] Password manager or secure vault used for secret sharing

---

## ?? Progress Tracker

| Phase | Status | Estimated Time | Actual Time |
|-------|--------|----------------|-------------|
| 1. Local Migration | ? Not Started | 10 min | ___ |
| 2. Git Security | ? Not Started | 30 min | ___ |
| 3. Credential Rotation | ? Not Started | 2 hours | ___ |
| 4. Production Setup | ? Not Started | 30 min | ___ |
| 5. Team Communication | ? Not Started | 15 min | ___ |
| 6. Verification | ? Not Started | 20 min | ___ |

**Completion Status**: ___% Complete

---

## ?? Rollback Plan (If Things Go Wrong)

### Local Development Issues
1. Clear User Secrets: `dotnet user-secrets clear`
2. Re-run migration: `.\migrate-secrets-ONCE.ps1`
3. If script deleted, manually set secrets using `setup-user-secrets.ps1`

### Production Issues
1. Check Render logs for specific error
2. Verify environment variables are set correctly
3. Test connection strings individually
4. Rollback deployment if needed

---

## ?? Support Contacts

### Technical Issues
- **Database**: Neon Support (https://neon.tech/docs/introduction)
- **M-Pesa**: Safaricom Developer Portal (https://developer.safaricom.co.ke/)
- **AWS**: AWS Support Console
- **Email Services**: Brevo/Zoho support portals

### Internal
- **Tech Lead**: [Your Tech Lead Contact]
- **DevOps**: [Your DevOps Contact]
- **Project Channel**: [Your Slack/Teams Channel]

---

## ?? Success Criteria

You're done when:
- ? All secrets removed from Git
- ? All credentials rotated
- ? App works locally with User Secrets
- ? App works in production with Environment Variables
- ? Team is informed and trained
- ? Documentation is complete
- ? All tests pass

---

## ?? Timeline

**Start Date**: _____________  
**Target Completion**: _____________ (Recommended: within 1 business day)  
**Actual Completion**: _____________

---

**Remember**: Security is not optional. Complete ALL phases, especially credential rotation! ??
