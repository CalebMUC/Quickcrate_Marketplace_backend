# ?? QUICK START: Verify Slug in All Product Endpoints

## ? **TL;DR - All Endpoints Already Return Slug!**

**Good News:** No code changes needed! All product GET endpoints automatically return the `slug` property through DTOs.

---

## ?? **Quick Setup (3 Steps)**

### **Step 1: Run Database Migration**
```bash
psql -U postgres -d your_database -f Migrations/20260215_Add_SlugRedirects_Table.sql
```

### **Step 2: Generate Slugs for Existing Products**
```sql
UPDATE "Products"
SET "Slug" = LOWER(REGEXP_REPLACE("ProductName" || '-' || SUBSTRING(CAST("ProductId" AS TEXT), 1, 8), '[^a-z0-9]+', '-', 'g')),
    "SlugUpdatedAt" = NOW()
WHERE "Slug" IS NULL AND "IsDeleted" = FALSE;
```

### **Step 3: Test an Endpoint**
```bash
curl http://localhost:5000/api/Product/GetFeaturedProducts?count=5
```

**Expected:** Each product should have `"slug": "product-name-abc12345"`

---

## ?? **All Endpoints That Return Slug**

| Endpoint | Method | Returns Slug? |
|----------|--------|---------------|
| `/api/Product/{id}` | GET | ? Yes (in `ProductResponseDto`) |
| `/api/Product/GetProduct/{id}` | GET | ? Yes (in `ProductResponseDto`) |
| `/api/Product/slug/{slug}` | GET | ? Yes (in `ProductResponseDto`) |
| `/api/Product/GetFeaturedProducts` | GET | ? Yes (in each `ProductListDto`) |
| `/api/Product/search` | POST | ? Yes (in each `ProductListDto`) |
| `/api/Product/category/{id}` | POST | ? Yes (in each `ProductListDto`) |
| `/api/Product/subcategory/{id}` | POST | ? Yes (in each `ProductListDto`) |
| `/api/Product/GetAllProducts` | GET | ? Yes (in each `ProductListDto`) |

---

## ?? **Quick Tests**

### **Test 1: Get Featured Products**
```bash
curl http://localhost:5000/api/Product/GetFeaturedProducts?count=5 | jq '.data[0].slug'
```
**Expected Output:** `"product-name-abc12345"`

### **Test 2: Get Product by Slug**
```bash
curl http://localhost:5000/api/Product/slug/getac-k120-core-i5-6949aa56 | jq '.data.slug'
```
**Expected Output:** `"getac-k120-core-i5-6949aa56"`

### **Test 3: Search Products**
```bash
curl -X POST http://localhost:5000/api/Product/search \
  -H "Content-Type: application/json" \
  -d '{"PageSize":10,"Page":1}' | jq '.data.items[0].slug'
```
**Expected Output:** `"some-product-name-xyz98765"`

---

## ?? **Verify Database Setup**

```sql
-- Check SlugRedirects table
SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'SlugRedirects';
-- Expected: 1

-- Check Products has Slug column
SELECT COUNT(*) FROM information_schema.columns 
WHERE table_name = 'Products' AND column_name = 'Slug';
-- Expected: 1

-- Check products with slugs
SELECT COUNT(*) FROM "Products" WHERE "Slug" IS NOT NULL;
-- Expected: > 0
```

---

## ?? **Example Response**

```json
{
  "success": true,
  "message": "Retrieved 5 featured products successfully",
  "data": [
    {
      "productId": "6949aa56-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
      "productName": "GETAC K120 Core i5",
      "slug": "getac-k120-core-i5-6949aa56",  ? SLUG HERE!
      "metaTitle": "GETAC K120 Core i5 - Rugged Laptop",
      "metaDescription": "High-performance rugged laptop...",
      "price": 1200.00,
      "categoryName": "Laptops",
      "isFeatured": true,
      "isActive": true
    }
  ]
}
```

---

## ?? **Troubleshooting**

### **Problem: Slug is `null` in response**
**Solution:**
```sql
-- Generate slugs for products
UPDATE "Products"
SET "Slug" = LOWER(REGEXP_REPLACE("ProductName" || '-' || SUBSTRING(CAST("ProductId" AS TEXT), 1, 8), '[^a-z0-9]+', '-', 'g'))
WHERE "Slug" IS NULL AND "IsDeleted" = FALSE;
```

### **Problem: "relation 'SlugRedirects' does not exist"**
**Solution:** Run the database migration:
```bash
psql -f Migrations/20260215_Add_SlugRedirects_Table.sql
```

### **Problem: Slug not appearing in DTO**
**Solution:** 
1. Verify AutoMapper is registered in `Program.cs`
2. Check ProductMappingProfile includes `Slug` mapping
3. Restart the API

---

## ?? **Success Criteria**

? Database has `SlugRedirects` table  
? `Products` table has `Slug` column  
? All existing products have generated slugs  
? `/api/Product/slug/{slug}` endpoint returns 200 OK  
? All product endpoints include `slug` in responses  
? New products auto-generate slugs on creation  
? Updating product name creates redirect and new slug  

---

## ?? **Need Help?**

See detailed documentation in:
- `SEO_SLUG_ENDPOINTS_VERIFICATION.md` - Full endpoint details
- `SEO_IMPLEMENTATION_COMPLETE.md` - Complete implementation guide
- `SEO_DATABASE_SETUP_GUIDE.md` - Database setup instructions

Run test scripts:
- PowerShell: `.\test-slug-endpoints.ps1`
- Bash: `./test-slug-endpoints.sh`

---

**Status:** ? **READY - All endpoints return slug automatically!**

**Just run the database migration and verify with tests!**
