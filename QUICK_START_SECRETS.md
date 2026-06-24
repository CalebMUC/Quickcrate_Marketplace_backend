# ?? Quick Start Guide - Secret Management

## For New Developers (Local Setup)

### Step 1: Clone the Repository
```bash
git clone https://github.com/CalebMUC/OrderApi
cd OrderApi
```

### Step 2: Set Up User Secrets (Choose One)

#### Option A: Interactive Script (Easiest)
```powershell
.\setup-user-secrets.ps1
```

#### Option B: Visual Studio
1. Right-click on `MinimartApi` project
2. Select **Manage User Secrets**
3. Copy content from `appsettings.Template.json`
4. Replace placeholders with actual values
5. Save the file

#### Option C: Command Line
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "JwtSettings:Secret" "your-jwt-secret"
# ... add other secrets
```

### Step 3: Run the Application
```bash
dotnet run
```

---

## For Existing Team Members (One-Time Migration)

```powershell
# 1. Run migration script (only once)
.\migrate-secrets-ONCE.ps1

# 2. Verify secrets
dotnet user-secrets list

# 3. Test application
dotnet run

# 4. Delete migration script
Remove-Item migrate-secrets-ONCE.ps1
```

---

## For DevOps/Production Deployment

### Render.com Setup

1. Go to **Render Dashboard** ? Your Service ? **Environment**
2. Copy variables from `PRODUCTION_ENV_SETUP.md`
3. Update with rotated credentials
4. Mark as **Secret**
5. Deploy

### Environment Variable Format
```bash
ConnectionStrings__DefaultConnection=your-value
JwtSettings__Secret=your-value
AWS__AccessKey=your-value
```

---

## Common Commands

### View Secrets
```bash
dotnet user-secrets list
```

### Add Secret
```bash
dotnet user-secrets set "Key:SubKey" "Value"
```

### Remove Secret
```bash
dotnet user-secrets remove "Key:SubKey"
```

### Clear All Secrets
```bash
dotnet user-secrets clear
```

---

## File Reference

| File | Purpose | Commit to Git? |
|------|---------|----------------|
| `appsettings.json` | Safe defaults only | ? Yes |
| `appsettings.Template.json` | Developer template | ? Yes |
| User Secrets (secrets.json) | Local secrets | ? No (auto-excluded) |
| `SECRET_MANAGEMENT_GUIDE.md` | Full documentation | ? Yes |
| `PRODUCTION_ENV_SETUP.md` | Production guide | ? Yes |
| `setup-user-secrets.ps1` | Setup script | ? Yes |
| `migrate-secrets-ONCE.ps1` | One-time migration | ?? Delete after use |

---

## Need Help?

- ?? Full guide: `SECRET_MANAGEMENT_GUIDE.md`
- ?? Production setup: `PRODUCTION_ENV_SETUP.md`
- ?? Implementation summary: `SECRET_IMPLEMENTATION_SUMMARY.md`
- ?? Microsoft docs: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets

---

## Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| "Connection string not found" | Run `dotnet user-secrets list` to verify secrets are set |
| "User Secrets not loading" | Ensure you're in **Development** environment |
| "Still seeing old values" | Delete `bin/` and `obj/` folders, rebuild |
| "Can't find secrets.json" | It's at `%APPDATA%\Microsoft\UserSecrets\95e4bf66-ce97-40ba-9993-4a85a220bc9d\` |

---

**Remember**: Secrets in User Secrets (local) or Environment Variables (production) override `appsettings.json`
