# ?? SEO Slug Implementation - Remaining Steps

## ? **What's Already Complete:**
1. ? SlugRedirect Model created
2. ? SlugService created (ISlugService + SlugService)
3. ? DbContext updated (DbSet added + configuration)
4. ? Product Model updated (5 SEO properties added)
5. ? Program.cs updated (SlugService registered)
6. ? IProductService updated (2 methods added)
7. ? ProductService updated (2 methods implemented)
8. ? IProductRepository updated (2 methods added)

---

## ? **What's Still Missing:**

### **1. ProductRepository Implementation** (CRITICAL)
### **2. ProductController New Endpoints**
### **3. DTO Updates**
### **4. Database Migration**

---

## ?? **STEP 8: Add to ProductRepository.cs**

### **8.1: Add SlugService to Constructor**

Find the ProductRepository constructor (around line 20-50) and update it:

```csharp
// ADD THIS USING
using Minimart_Api.Services.SlugService;

public class ProductRepository : IProductRepository
{
    private readonly MinimartDBContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductRepository> _logger;
    private readonly ISlugService _slugService; // ADD THIS

    public ProductRepository(
        MinimartDBContext context,
        IMapper mapper,
        ILogger<ProductRepository> logger,
        ISlugService slugService) // ADD THIS PARAMETER
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _slugService = slugService; // ADD THIS ASSIGNMENT
    }
```

---

### **8.2: Implement GetProductBySlugAsync Method**

Add this method at the end of ProductRepository.cs (before the closing brace):

```csharp
// ==========================
// SEO SLUG MANAGEMENT
// ==========================

/// <summary>
/// Get product by SEO-friendly slug
/// </summary>
public async Task<ProductResponseDto?> GetProductBySlugAsync(string slug)
{
    try
    {
        _logger.LogInformation("Fetching product by slug: {Slug}", slug);

        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.SubSubCategory)
            .Include(p => p.Merchant)
            .Where(p => p.Slug == slug && !p.IsDeleted && p.IsActive)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            // Check slug redirects table for old slugs
            var redirect = await _context.SlugRedirects
                .Where(sr => sr.OldSlug == slug)
                .OrderByDescending(sr => sr.CreatedAt)
                .FirstOrDefaultAsync();

            if (redirect != null)
            {
                _logger.LogInformation("Found redirect from {OldSlug} to {NewSlug}", 
                    slug, redirect.NewSlug);
                    
                // Fetch product using new slug
                product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .Include(p => p.SubSubCategory)
                    .Include(p => p.Merchant)
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

/// <summary>
/// Update product slug (called when product name changes)
/// </summary>
public async Task<bool> UpdateProductSlugAsync(Guid productId, string newSlug, string updatedBy)
{
    try
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return false;

        var oldSlug = product.Slug;

        // Update slug
        product.Slug = newSlug;
        product.SlugUpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = updatedBy;
        product.UpdatedOn = DateTime.UtcNow;

        // Store old slug in redirects table
        if (!string.IsNullOrEmpty(oldSlug) && oldSlug != newSlug)
        {
            var redirect = new SlugRedirect
            {
                OldSlug = oldSlug,
                NewSlug = newSlug,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            };

            _context.SlugRedirects.Add(redirect);
        }

        await _context.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating slug for product {ProductId}", productId);
        return false;
    }
}
```

---

### **8.3: Update CreateAsync Method**

Find the `CreateAsync` method in ProductRepository and add slug generation:

```csharp
public async Task<ProductResponseDto> CreateAsync(CreateProductDto createProductDto, string createdBy)
{
    // ... existing code to create product object ...

    var product = new Product
    {
        ProductId = Guid.NewGuid(),
        ProductName = createProductDto.ProductName,
        // ... other properties ...
    };

    // **ADD THIS CODE - Generate SEO-friendly slug**
    product.Slug = _slugService.GenerateSlug(createProductDto.ProductName, product.ProductId);
    product.SlugUpdatedAt = DateTime.UtcNow;

    // Auto-generate meta tags for SEO
    product.MetaTitle = $"{createProductDto.ProductName} | Buy in Kenya | QuickCrate";
    
    product.MetaDescription = string.IsNullOrEmpty(createProductDto.Description) 
        ? $"Buy {createProductDto.ProductName} in Kenya at QuickCrate" 
        : createProductDto.Description.Length > 250 
            ? createProductDto.Description.Substring(0, 247) + "..." 
            : createProductDto.Description;

    product.MetaKeywords = $"{createProductDto.ProductName}, Kenya, QuickCrate, " +
                          $"{createProductDto.CategoryName}, Buy Online";

    // ... rest of existing code ...
    _context.Products.Add(product);
    await _context.SaveChangesAsync();

    return _mapper.Map<ProductResponseDto>(product);
}
```

---

### **8.4: Update UpdateAsync Method** (Optional but Recommended)

Find the `UpdateAsync` method and add slug regeneration when product name changes:

```csharp
public async Task<ProductResponseDto> UpdateAsync(UpdateProductDto updateProductDto, string updatedBy)
{
    var product = await _context.Products.FindAsync(updateProductDto.ProductId);
    
    if (product == null)
        throw new KeyNotFoundException($"Product with ID {updateProductDto.ProductId} not found");

    // Check if product name changed
    bool nameChanged = product.ProductName != updateProductDto.ProductName;

    // Update product properties
    product.ProductName = updateProductDto.ProductName;
    // ... update other properties ...

    // **ADD THIS CODE - Regenerate slug if name changed**
    if (nameChanged)
    {
        var oldSlug = product.Slug;
        var newSlug = _slugService.GenerateSlug(updateProductDto.ProductName, product.ProductId);

        product.Slug = newSlug;
        product.SlugUpdatedAt = DateTime.UtcNow;

        // Store old slug in redirects table
        if (!string.IsNullOrEmpty(oldSlug) && oldSlug != newSlug)
        {
            var redirect = new SlugRedirect
            {
                OldSlug = oldSlug,
                NewSlug = newSlug,
                ProductId = product.ProductId,
                CreatedAt = DateTime.UtcNow
            };

            _context.SlugRedirects.Add(redirect);
        }

        // Update meta tags
        product.MetaTitle = $"{updateProductDto.ProductName} | Buy in Kenya | QuickCrate";
        product.MetaDescription = string.IsNullOrEmpty(updateProductDto.Description) 
            ? $"Buy {updateProductDto.ProductName} in Kenya at QuickCrate" 
            : updateProductDto.Description.Length > 250 
                ? updateProductDto.Description.Substring(0, 247) + "..." 
                : updateProductDto.Description;
    }

    product.UpdatedBy = updatedBy;
    product.UpdatedOn = DateTime.UtcNow;

    await _context.SaveChangesAsync();
    return _mapper.Map<ProductResponseDto>(product);
}
```

---

## ?? **STEP 9: Add New Controller Endpoints**

### **9.1: Add SlugService to ProductController Constructor**

```csharp
using Minimart_Api.Services.SlugService; // ADD THIS

private readonly IProductService _productService;
private readonly ILogger<ProductController> _logger;
private readonly ICurrentUserService _currentUserService;
private readonly ISlugService _slugService; // ADD THIS

public ProductController(
    IProductService productService,
    ICurrentUserService currentUserService,
    ILogger<ProductController> logger,
    ISlugService slugService) // ADD THIS PARAMETER
{
    _productService = productService;
    _currentUserService = currentUserService;
    _logger = logger;
    _slugService = slugService; // ADD THIS ASSIGNMENT
}
```

---

### **9.2: Add GetProductBySlug Endpoint**

Add this method to ProductController.cs:

```csharp
/// <summary>
/// Get product by SEO-friendly slug
/// GET /api/Product/slug/getac-k120-core-i5-6949aa56
/// </summary>
[HttpGet("slug/{slug}")]
[ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), 200)]
[ProducesResponseType(404)]
public async Task<IActionResult> GetProductBySlug(string slug)
{
    try
    {
        _logger.LogInformation("Getting product by slug: {Slug}", slug);

        // Validate slug format
        if (!_slugService.IsValidSlug(slug))
        {
            _logger.LogWarning("Invalid slug format: {Slug}", slug);
            return BadRequest(ApiResponse<object>.CreateError("Invalid product URL format"));
        }

        // Try to find product by slug
        var product = await _productService.GetProductBySlugAsync(slug);

        if (product == null)
        {
            _logger.LogWarning("Product not found for slug: {Slug}", slug);
            return NotFound(ApiResponse<object>.CreateError($"Product not found"));
        }

        _logger.LogInformation("Successfully retrieved product by slug: {Slug} -> ProductId: {ProductId}", 
            slug, product.ProductId);

        return Ok(ApiResponse<ProductResponseDto>.CreateSuccess(
            product, 
            "Product retrieved successfully"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving product by slug: {Slug}", slug);
        return StatusCode(500, ApiResponse<object>.CreateError(
            "An error occurred while retrieving the product"));
    }
}
```

---

### **9.3: Update Existing GetProduct Method (301 Redirect)**

Find the existing `GetProduct` method and update it:

```csharp
/// <summary>
/// Get a specific product by ID (Enhanced endpoint with 301 redirect to slug URL)
/// </summary>
[HttpGet("GetProduct/{productId:guid}")]
public async Task<IActionResult> GetProduct(Guid productId)
{
    try
    {
        _logger.LogInformation("Getting product details for ProductId: {ProductId}", productId);

        var product = await _productService.GetProductAsync(productId);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            return NotFound(ApiResponse<object>.CreateError($"Product with ID {productId} not found"));
        }

        // **NEW: 301 Redirect to slug-based URL if slug exists**
        if (!string.IsNullOrEmpty(product.Slug))
        {
            _logger.LogInformation(
                "Redirecting ProductId {ProductId} to slug URL: {Slug}", 
                productId, product.Slug);
                
            return RedirectPermanent($"/api/Product/slug/{product.Slug}");
        }

        _logger.LogInformation("Successfully retrieved product: {ProductId}", productId);
        return Ok(ApiResponse<ProductResponseDto>.CreateSuccess(product, "Product retrieved successfully"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
        return StatusCode(500, ApiResponse<object>.CreateError("An error occurred while retrieving the product"));
    }
}
```

---

## ?? **STEP 10: Update DTOs**

### **10.1: Update ProductResponseDto**

Find `DTOS/Products/ProductResponseDto.cs` or `BaseProductDto.cs` and add:

```csharp
public class ProductResponseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    
    // **ADD THESE SEO PROPERTIES**
    public string? Slug { get; set; }
    public DateTime? SlugUpdatedAt { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    
    // ... rest of existing properties ...
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    // etc.
}
```

---

### **10.2: Update ProductMappingProfile (AutoMapper)**

Find `Mappings/ProductMappingProfile.cs` and ensure it maps the new properties:

```csharp
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.SlugUpdatedAt, opt => opt.MapFrom(src => src.SlugUpdatedAt))
            .ForMember(dest => dest.MetaTitle, opt => opt.MapFrom(src => src.MetaTitle))
            .ForMember(dest => dest.MetaDescription, opt => opt.MapFrom(src => src.MetaDescription))
            .ForMember(dest => dest.MetaKeywords, opt => opt.MapFrom(src => src.MetaKeywords));
            // ... other mappings ...
    }
}
```

---

## ?? **STEP 11: Run Database Migration**

### **Option 1: Using EF Core Migrations (Recommended)**

```powershell
# Add migration
dotnet ef migrations add AddSEOSlugToProducts

# Update database
dotnet ef database update
```

### **Option 2: Run SQL Script Directly**

Execute this PostgreSQL script:

```sql
-- Add SEO columns to Products table
ALTER TABLE "Products" 
ADD COLUMN IF NOT EXISTS "Slug" VARCHAR(300),
ADD COLUMN IF NOT EXISTS "SlugUpdatedAt" TIMESTAMP,
ADD COLUMN IF NOT EXISTS "MetaTitle" VARCHAR(150),
ADD COLUMN IF NOT EXISTS "MetaDescription" VARCHAR(300),
ADD COLUMN IF NOT EXISTS "MetaKeywords" VARCHAR(500);

-- Create unique index on Slug
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Products_Slug" 
ON "Products"("Slug") 
WHERE "Slug" IS NOT NULL;

-- Create PostgreSQL function to generate slugs
CREATE OR REPLACE FUNCTION generate_product_slug(
    product_name TEXT,
    product_id UUID
) RETURNS VARCHAR(300) AS $$
DECLARE
    slug VARCHAR(300);
    short_id VARCHAR(8);
BEGIN
    -- Get last 8 characters of UUID
    short_id := SUBSTRING(product_id::TEXT FROM 25 FOR 8);
    
    -- Generate slug: lowercase, replace spaces/special chars, append short ID
    slug := LOWER(TRIM(
        REGEXP_REPLACE(
            REGEXP_REPLACE(product_name, '[^a-zA-Z0-9\s-]', '', 'g'),
            '\s+', '-', 'g'
        )
    )) || '-' || short_id;
    
    RETURN slug;
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Generate slugs for all existing products
UPDATE "Products"
SET 
    "Slug" = generate_product_slug("ProductName", "ProductId"),
    "SlugUpdatedAt" = NOW(),
    "MetaTitle" = "ProductName" || ' | Buy in Kenya | QuickCrate',
    "MetaDescription" = CASE 
        WHEN LENGTH("Description") > 250 
        THEN SUBSTRING("Description" FROM 1 FOR 247) || '...'
        ELSE "Description"
    END
WHERE "Slug" IS NULL AND "ProductName" IS NOT NULL;

-- Verify
SELECT 
    COUNT(*) as total_products,
    COUNT("Slug") as products_with_slug,
    COUNT(*) - COUNT("Slug") as missing_slugs
FROM "Products";
```

---

## ? **COMPLETION CHECKLIST**

- [ ] **ProductRepository**: SlugService added to constructor
- [ ] **ProductRepository**: GetProductBySlugAsync implemented
- [ ] **ProductRepository**: UpdateProductSlugAsync implemented
- [ ] **ProductRepository**: CreateAsync updated (slug generation)
- [ ] **ProductRepository**: UpdateAsync updated (slug regeneration)
- [ ] **ProductController**: SlugService added to constructor
- [ ] **ProductController**: GetProductBySlug endpoint added
- [ ] **ProductController**: GetProduct updated (301 redirect)
- [ ] **DTOs**: ProductResponseDto updated (5 SEO properties)
- [ ] **Mappings**: ProductMappingProfile updated
- [ ] **Database**: Migration applied (all products have slugs)

---

## ?? **TESTING**

### **Test 1: Verify Slugs Generated**

```sql
SELECT "ProductId"::TEXT, "ProductName", "Slug", LENGTH("Slug") as slug_length
FROM "Products"
LIMIT 10;
```

### **Test 2: Test New Endpoint**

```
GET https://orderapi-33pp.onrender.com/api/Product/slug/getac-k120-core-i5-6949aa56
```

### **Test 3: Test 301 Redirect**

```
GET https://orderapi-33pp.onrender.com/api/Product/GetProduct/6949aa56-c67c-48ce-8086-1b54f7c6c748
```

**Expected:** HTTP 301 ? `/api/Product/slug/getac-k120-core-i5-6949aa56`

---

## ?? **DEPLOYMENT**

1. Commit all changes
2. Push to repository
3. Deploy to Render
4. Run database migration
5. Test endpoints in production

---

**All missing implementation steps are now documented! Follow each step carefully.** ??
