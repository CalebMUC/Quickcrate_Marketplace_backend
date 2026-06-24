# ?? Quick Testing Guide - Social Media Share

## ? **Pre-Deployment Testing (Localhost)**

### **1. Start Your API**
```bash
cd E:\MinimartApi_Staging\OrderApi
dotnet run
```

### **2. Test Crawler Response**

```bash
# Windows PowerShell
Invoke-WebRequest -Uri "https://localhost:7134/p/your-product-slug" `
  -Headers @{"User-Agent"="WhatsApp/2.23.20"} | Select-Object Content

# Linux/Mac
curl -H "User-Agent: WhatsApp/2.23.20" https://localhost:7134/p/your-product-slug
```

**Expected:** HTML with Open Graph meta tags

### **3. Test Human Redirect**

```bash
# PowerShell
Invoke-WebRequest -Uri "https://localhost:7134/p/your-product-slug" `
  -Headers @{"User-Agent"="Mozilla/5.0"} -MaximumRedirection 0

# curl
curl -L -H "User-Agent: Mozilla/5.0" https://localhost:7134/p/your-product-slug
```

**Expected:** 302 Redirect to `http://localhost:3000/p/your-product-slug`

---

## ?? **Post-Deployment Testing (Render)**

### **1. Test Live Endpoint**

```bash
# Get a real product slug first
curl https://orderapi-33pp.onrender.com/api/Product/GetFeaturedProducts?count=1 | jq '.[0].slug'

# Test crawler response
curl -H "User-Agent: WhatsApp/2.23.20" \
  https://orderapi-33pp.onrender.com/p/YOUR_PRODUCT_SLUG
```

### **2. Facebook Sharing Debugger**

1. Go to: https://developers.facebook.com/tools/debug/
2. Paste your URL:
   ```
   https://orderapi-33pp.onrender.com/p/YOUR_PRODUCT_SLUG
   ```
3. Click "Debug"

**Expected Results:**
- ? `og:title` - Product name
- ? `og:description` - Product description
- ? `og:image` - Product image URL
- ? `og:type` - "product"
- ? `product:price:amount` - Price

### **3. Twitter Card Validator**

1. Go to: https://cards-dev.twitter.com/validator
2. Paste your URL
3. Click "Preview card"

**Expected:** Large image card with product details

---

## ?? **WhatsApp Testing**

### **Method 1: Direct Share**

1. Open WhatsApp (mobile or web)
2. Start a chat
3. Paste the link:
   ```
   https://orderapi-33pp.onrender.com/p/YOUR_PRODUCT_SLUG
   ```
4. Wait 2-3 seconds

**Expected:**
- ? Image preview appears
- ? Product name as title
- ? Price shown
- ? "QuickCrate" as site name

### **Method 2: Browser Share Button**

1. Open product page in mobile browser:
   ```
   https://quickcrate.co.ke/p/YOUR_PRODUCT_SLUG
   ```
2. Click share button (if implemented)
3. Choose WhatsApp
4. Preview should appear

---

## ?? **Debugging**

### **Check Render Logs**

```bash
# View real-time logs
# Go to Render Dashboard ? Your Service ? Logs

# Look for:
"Share page requested for slug: your-slug"
"Serving crawler HTML for slug: your-slug, User-Agent: WhatsApp..."
"Redirecting human user to: https://quickcrate.co.ke/p/your-slug"
```

### **Check OG Tags in HTML**

```bash
curl -H "User-Agent: WhatsApp/2.23.20" \
  https://orderapi-33pp.onrender.com/p/YOUR_PRODUCT_SLUG \
  | grep -i "og:"
```

**Expected Output:**
```html
<meta property="og:type" content="product" />
<meta property="og:title" content="Product Name" />
<meta property="og:image" content="https://..." />
...
```

---

## ? **Checklist**

### **Before Deployment:**
- [ ] Constructor added to ShareController
- [ ] Error handling implemented
- [ ] Configuration values set in appsettings.json
- [ ] Build successful
- [ ] Local testing passed

### **After Deployment:**
- [ ] Render deployment successful
- [ ] No errors in Render logs
- [ ] Crawler test returns HTML
- [ ] Human test redirects to frontend
- [ ] Facebook debugger shows all OG tags
- [ ] WhatsApp preview working
- [ ] Twitter card working
- [ ] Images loading correctly

---

## ?? **Common Issues**

### **Issue 1: No preview in WhatsApp**

**Causes:**
- Image URL not accessible
- Image too large (> 5MB)
- Wrong image dimensions

**Solutions:**
- Verify image URL in browser
- Compress images
- Use 1200x630px images

### **Issue 2: Old preview showing**

**Cause:** WhatsApp/Facebook cache

**Solution:**
- Clear Facebook cache: https://developers.facebook.com/tools/debug/ ? "Scrape Again"
- WhatsApp: Delete and resend link

### **Issue 3: Redirect loop**

**Cause:** Frontend calling backend share endpoint

**Solution:**
- Frontend should call: `https://quickcrate.co.ke/p/{slug}`
- Share button should use: `https://orderapi-33pp.onrender.com/p/{slug}`

---

## ?? **Expected Timings**

| Action | Expected Time |
|--------|---------------|
| First request (cold start) | 2-5 seconds |
| Cached request | < 100ms |
| WhatsApp preview fetch | 1-3 seconds |
| Redirect to frontend | < 200ms |

---

## ?? **Success Criteria**

? **All these should work:**

1. **WhatsApp share shows:**
   - Product image
   - Product name
   - Price
   - QuickCrate branding

2. **Facebook share shows:**
   - Same as WhatsApp
   - All OG tags in debugger

3. **Twitter share shows:**
   - Large image card
   - Product details

4. **Human users:**
   - Get redirected to React app
   - See product page normally
   - Can share via native button

5. **SEO:**
   - Google sees structured data
   - Product rich snippets appear

---

## ?? **Quick Test Script**

```bash
# Save as test-share.sh

#!/bin/bash

PRODUCT_SLUG="your-product-slug"
API_URL="https://orderapi-33pp.onrender.com"

echo "Testing Share Controller..."
echo ""

# Test 1: Crawler
echo "1. Testing WhatsApp crawler..."
curl -s -H "User-Agent: WhatsApp/2.23.20" "$API_URL/p/$PRODUCT_SLUG" | grep -q "og:title"
if [ $? -eq 0 ]; then
  echo "? Crawler test PASSED"
else
  echo "? Crawler test FAILED"
fi

# Test 2: Human redirect
echo "2. Testing human redirect..."
REDIRECT=$(curl -s -o /dev/null -w "%{redirect_url}" -H "User-Agent: Mozilla/5.0" "$API_URL/p/$PRODUCT_SLUG")
if [[ $REDIRECT == *"quickcrate.co.ke"* ]]; then
  echo "? Redirect test PASSED"
else
  echo "? Redirect test FAILED"
fi

echo ""
echo "Done!"
```

**Run:**
```bash
chmod +x test-share.sh
./test-share.sh
```

---

**Need help?** Check `SOCIAL_MEDIA_SHARE_IMPLEMENTATION.md` for detailed guide.
