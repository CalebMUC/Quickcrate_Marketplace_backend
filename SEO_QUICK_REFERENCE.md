# ?? SEO Slug Implementation - Quick Reference

## ? **What's Done:**
- SlugRedirect model ?
- SlugService complete ?
- Product model updated ?
- DbContext configured ?
- Services registered ?
- Interfaces updated ?

## ? **What's Left:**
- ProductRepository implementation (4 code blocks)
- ProductController updates (3 code blocks)
- DTO updates (2 small changes)
- Database migration (1 SQL script)

---

## ?? **Copy-Paste Implementation (In Order)**

### **1. ProductRepository.cs - Add to Constructor** (Line ~20)

```csharp
using Minimart_Api.Services.SlugService; // ADD THIS

private readonly ISlugService _slugService; // ADD THIS

public ProductRepository(
    MinimartDBContext context,
    IMapper mapper,
    ILogger<ProductRepository> logger,
    ISlugService slugService) // ADD THIS
{
    _context = context;
    _mapper = mapper;
    _logger = logger;
    _slugService = slugService; // ADD THIS
}
```

---

### **2. ProductRepository.cs - Add at END of file** (Before last `}`)

```csharp
// ==========================
// SEO SLUG MANAGEMENT
// ==========================

public async Task<ProductResponseDto?> GetProductBySlugAsync(string slug)
{
    try
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Where(p => p.Slug == slug && !p.IsDeleted && p.IsActive)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            var redirect = await _context.SlugRedirects
                .Where(sr => sr.OldSlug == slug)
                .OrderByDescending(sr => sr.CreatedAt)
                .FirstOrDefaultAsync();

            if (redirect != null)
            {
                product = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.Slug == redirect.NewSlug && !p.IsDeleted && p.IsActive)
                    .FirstOrDefaultAsync();
            }
        }

        return product != null ? _mapper.Map<ProductResponseDto>(product) : null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching product by slug: {Slug}", slug);
        return null;
    }
}

public async Task<bool> UpdateProductSlugAsync(Guid productId, string newSlug, string updatedBy)
{
    try
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return false;

        var oldSlug = product.Slug;
        product.Slug = newSlug;
        product.SlugUpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(oldSlug) && oldSlug != newSlug)
        {
            _context.SlugRedirects.Add(new SlugRedirect
            {
                OldSlug = oldSlug,
                NewSlug = newSlug,
                ProductId = productId
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating slug");
        return false;
    }
}
```

---

### **3. ProductRepository.cs - Update CreateAsync** (Find existing method)

**ADD after creating product object, BEFORE _context.Products.Add():**

```csharp
// Generate SEO slug
product.Slug = _slugService.GenerateSlug(createProductDto.ProductName, product.ProductId);
product.SlugUpdatedAt = DateTime.UtcNow;
product.MetaTitle = $"{createProductDto.ProductName} | Buy in Kenya | QuickCrate";
product.MetaDescription = createProductDto.Description?.Length > 250 
    ? createProductDto.Description.Substring(0, 247) + "..." 
    : createProductDto.Description;
```

---

### **4. ProductController.cs - Add to Constructor**

```csharp
using Minimart_Api.Services.SlugService; // ADD THIS

private readonly ISlugService _slugService; // ADD THIS

public ProductController(
    IProductService productService,
    ICurrentUserService currentUserService,
    ILogger<ProductController> logger,
    ISlugService slugService) // ADD THIS
{
    _productService = productService;
    _currentUserService = currentUserService;
    _logger = logger;
    _slugService = slugService; // ADD THIS
}
```

---

### **5. ProductController.cs - Add New Endpoint**

```csharp
[HttpGet("slug/{slug}")]
public async Task<IActionResult> GetProductBySlug(string slug)
{
    if (!_slugService.IsValidSlug(slug))
        return BadRequest(ApiResponse<object>.CreateError("Invalid URL"));

    var product = await _productService.GetProductBySlugAsync(slug);
    
    if (product == null)
        return NotFound(ApiResponse<object>.CreateError("Product not found"));

    return Ok(ApiResponse<ProductResponseDto>.CreateSuccess(product));
}
```

---

### **6. ProductController.cs - Update GetProduct** (Find existing method)

**ADD after `if (product == null)` check:**

```csharp
// 301 Redirect to slug URL
if (!string.IsNullOrEmpty(product.Slug))
{
    return RedirectPermanent($"/api/Product/slug/{product.Slug}");
}
```

---

### **7. ProductResponseDto.cs** (or BaseProductDto.cs)

```csharp
// ADD these properties
public string? Slug { get; set; }
public DateTime? SlugUpdatedAt { get; set; }
public string? MetaTitle { get; set; }
public string? MetaDescription { get; set; }
public string? MetaKeywords { get; set; }
```

---

### **8. Database Migration** (PostgreSQL)

```sql
-- Add SEO columns
ALTER TABLE "Products" 
ADD COLUMN IF NOT EXISTS "Slug" VARCHAR(300),
ADD COLUMN IF NOT EXISTS "MetaTitle" VARCHAR(150),
ADD COLUMN IF NOT EXISTS "MetaDescription" VARCHAR(300);

-- Create index
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Products_Slug" ON "Products"("Slug");

-- Generate slugs for existing products
UPDATE "Products"
SET "Slug" = LOWER(REGEXP_REPLACE("ProductName", '[^a-zA-Z0-9\s-]', '', 'g')) || '-' || 
             SUBSTRING("ProductId"::TEXT FROM 25 FOR 8)
WHERE "Slug" IS NULL;
```

---

## ?? **Quick Test**

```bash
# 1. Build
dotnet build

# 2. Test new endpoint
curl https://orderapi-33pp.onrender.com/api/Product/slug/test-product-12345678

# 3. Test redirect
curl -I https://orderapi-33pp.onrender.com/api/Product/GetProduct/{guid}
# Should return: HTTP 301
```

---

## ? **Checklist**

- [ ] ProductRepository: SlugService added to constructor
- [ ] ProductRepository: 2 new methods added at end
- [ ] ProductRepository: CreateAsync updated (slug generation)
- [ ] ProductController: SlugService added to constructor
- [ ] ProductController: GetProductBySlug endpoint added
- [ ] ProductController: GetProduct updated (301 redirect)
- [ ] DTO: 5 properties added
- [ ] Database: Migration run
- [ ] Build: Successful
- [ ] Tests: Passing

---

## ?? **Common Issues**

### Issue: "ISlugService not found"
**Fix:** Add `using Minimart_Api.Services.SlugService;`

### Issue: "SlugRedirect not found"
**Fix:** Rebuild solution, check Models/SlugRedirect.cs exists

### Issue: "Slug is always null in database"
**Fix:** Run database migration script (step 8)

---

## ?? **Time:** ~1 hour total

**Follow steps 1-8 in order. Copy-paste code exactly as shown.** ??

---

## ?? **Full Documentation**

For detailed explanations, see:
- `SEO_SLUG_REMAINING_IMPLEMENTATION.md` - Complete guide
- `SEO_IMPLEMENTATION_STATUS.md` - Progress tracker
