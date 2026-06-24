# ?? Quick Deployment Guide - Slug Fix

## ? **Changes Applied**

1. ? `Models/SlugRedirect.cs` - Changed `Id` to `RedirectId`
2. ? `Data/MinimartDBContext.cs` - Updated configuration
3. ? Build successful

---

## ?? **Deploy to Render**

### **Step 1: Commit and Push**

```bash
# Add changes
git add Models/SlugRedirect.cs Data/MinimartDBContext.cs

# Commit
git commit -m "Fix: Update SlugRedirect to use RedirectId column to match database schema"

# Push to your branch
git push origin Seo-optimization
```

### **Step 2: Merge to Main (if needed)**

```bash
# Switch to main
git checkout main

# Merge your branch
git merge Seo-optimization

# Push to main
git push origin main
```

### **Step 3: Render Auto-Deploy**

Render will automatically deploy when you push to main (or your connected branch).

**Monitor deployment:**
- Go to your Render dashboard
- Click on your service
- Watch the "Events" tab for deployment progress

---

## ?? **Test After Deployment**

### **1. Check Render Logs**

Look for successful startup:
```
Application started. Press Ctrl+C to shut down.
Hosting environment: Production
```

No errors about `column s.Id does not exist`

### **2. Test Slug Endpoint**

```bash
# Replace with your actual Render URL
curl https://your-app.onrender.com/api/Product/slug/gusalai-mens-quartz-stainless-steel-watch-2226a633

# Expected: 200 OK with product data
```

### **3. Test Product Creation**

```bash
POST https://your-app.onrender.com/api/Product/Create
Content-Type: application/json

{
  "productName": "Test Product",
  "price": 100,
  "merchantID": "your-merchant-guid",
  "categoryId": "your-category-guid"
}

# Response should include auto-generated slug
```

---

## ?? **Localhost Setup (If Needed)**

If your localhost database needs updating:

```sql
-- Option 1: Rename column
ALTER TABLE "SlugRedirects" RENAME COLUMN "Id" TO "RedirectId";

-- Option 2: Recreate table (run the migration script)
\i Migrations/20260215_Add_SlugRedirects_Table.sql
```

Then restart your local API.

---

## ? **Success Criteria**

- [ ] Code pushed to Git
- [ ] Render deployment successful
- [ ] No `column s.Id` errors in Render logs
- [ ] Slug endpoint returns 200 OK
- [ ] Product creation generates slug
- [ ] Product update creates redirects

---

## ?? **You're Done!**

The fix is deployed and working when all criteria are met.

**Monitor Render logs for the first few hours to ensure stability.**

---

**Last Updated:** February 15, 2026
