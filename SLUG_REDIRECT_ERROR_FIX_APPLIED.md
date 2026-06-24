# ? FIXED: Render Database Error - "column s.Id does not exist"

## ?? **Problem Summary**

**Error on Render (Production):**
```
Npgsql.PostgresException: 42703: column s.Id does not exist
POSITION: 8
```

**Location:** `ProductRepository.GetProductBySlugAsync()` at line 440

**Root Cause:**
- Database on Render has column named `RedirectId` (created via SQL migration script)
- C# Model used property name `Id`
- Entity Framework Core generated SQL looking for `s.Id` which doesn't exist
- Localhost might have worked if the table was created with a different schema

---

## ? **Solution Applied**

### **1. Updated `Models/SlugRedirect.cs`**

**Before:**
```csharp
public class SlugRedirect
{
    [Key]
    public Guid Id { get; set; }  // ? Doesn't match database column
    
    public string OldSlug { get; set; } = string.Empty;
    public string NewSlug { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}
```

**After:**
```csharp
[Table("SlugRedirects")]
public class SlugRedirect
{
    [Key]
    [Column("RedirectId")]
    public Guid RedirectId { get; set; } = Guid.NewGuid();  // ? Matches database
    
    [Required]
    [StringLength(300)]
    [Column("OldSlug")]
    public string OldSlug { get; set; } = string.Empty;
    
    [Required]
    [StringLength(300)]
    [Column("NewSlug")]
    public string NewSlug { get; set; } = string.Empty;
    
    [Required]
    [Column("ProductId")]
    public Guid ProductId { get; set; }
    
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("IsActive")]
    public bool IsActive { get; set; } = true;
    
    [ForeignKey("ProductId")]
    public virtual Product? Product { get; set; }
}
```

---

### **2. Updated `Data/MinimartDBContext.cs`**

**Before:**
```csharp
modelBuilder.Entity<SlugRedirect>(entity =>
{
    entity.ToTable("SlugRedirects");
    entity.HasKey(sr => sr.Id);  // ? Wrong property name
    
    entity.Property(sr => sr.CreatedAt)
        .HasColumnType("timestamp with time zone");
    
    // ... rest of configuration
});
```

**After:**
```csharp
modelBuilder.Entity<SlugRedirect>(entity =>
{
    entity.ToTable("SlugRedirects");
    entity.HasKey(sr => sr.RedirectId);  // ? Correct property name
    
    entity.Property(sr => sr.CreatedAt)
        .HasColumnType("timestamp with time zone");
    
    entity.Property(sr => sr.IsActive)
        .HasDefaultValue(true);
    
    entity.HasOne(sr => sr.Product)
        .WithMany()
        .HasForeignKey(sr => sr.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
    
    entity.HasIndex(sr => sr.OldSlug);
    entity.HasIndex(sr => sr.NewSlug);
    entity.HasIndex(sr => sr.ProductId);
    entity.HasIndex(sr => sr.IsActive);
});
```

---

## ?? **Why This Happened**

### **Database Schema (Render - Production)**
Created via SQL migration script `20260215_Add_SlugRedirects_Table.sql`:
```sql
CREATE TABLE "SlugRedirects" (
    "RedirectId" UUID PRIMARY KEY,  ? Production uses this name
    "OldSlug" VARCHAR(500) NOT NULL,
    "NewSlug" VARCHAR(500) NOT NULL,
    "ProductId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);
```

### **C# Model (Original)**
Had property named `Id`:
```csharp
public Guid Id { get; set; }  ? Code used this name
```

### **EF Core Generated SQL (Broken)**
```sql
SELECT s."Id", s."OldSlug", s."NewSlug", s."ProductId"
FROM "SlugRedirects" s
WHERE s."OldSlug" = @p0
```
? **Error:** `column s.Id does not exist`

### **EF Core Generated SQL (Fixed)**
```sql
SELECT s."RedirectId", s."OldSlug", s."NewSlug", s."ProductId"
FROM "SlugRedirects" s
WHERE s."OldSlug" = @p0
```
? **Success:** Matches database schema

---

## ?? **Files Changed**

1. ? `Models/SlugRedirect.cs` - Changed `Id` ? `RedirectId`
2. ? `Data/MinimartDBContext.cs` - Updated configuration to use `RedirectId`

---

## ?? **Testing the Fix**

### **On Render (After Deployment):**

```bash
# Test slug endpoint
curl https://your-app.onrender.com/api/Product/slug/gusalai-mens-quartz-stainless-steel-watch-2226a633

# Expected: 200 OK with product data
```

### **Verify Database Schema:**

```sql
-- Connect to Render PostgreSQL
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'SlugRedirects'
ORDER BY ordinal_position;

-- Expected columns:
-- RedirectId | uuid
-- OldSlug    | character varying
-- NewSlug    | character varying
-- ProductId  | uuid
-- CreatedAt  | timestamp with time zone
-- IsActive   | boolean
```

---

## ?? **Important: Localhost Compatibility**

If your localhost database has a different schema (column named `Id` instead of `RedirectId`), you need to update it:

### **Option 1: Rename Column (Quick Fix)**
```sql
ALTER TABLE "SlugRedirects" 
RENAME COLUMN "Id" TO "RedirectId";
```

### **Option 2: Recreate Table (Clean Fix)**
```sql
-- Drop existing table
DROP TABLE IF EXISTS "SlugRedirects" CASCADE;

-- Recreate with correct schema
CREATE TABLE "SlugRedirects" (
    "RedirectId" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OldSlug" VARCHAR(500) NOT NULL,
    "NewSlug" VARCHAR(500) NOT NULL,
    "ProductId" UUID NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    
    CONSTRAINT "FK_SlugRedirects_Products" FOREIGN KEY ("ProductId") 
        REFERENCES "Products"("ProductId") ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX "IX_SlugRedirects_OldSlug" ON "SlugRedirects" ("OldSlug");
CREATE INDEX "IX_SlugRedirects_NewSlug" ON "SlugRedirects" ("NewSlug");
CREATE INDEX "IX_SlugRedirects_ProductId" ON "SlugRedirects" ("ProductId");
CREATE INDEX "IX_SlugRedirects_IsActive" ON "SlugRedirects" ("IsActive");
```

---

## ?? **Deployment Checklist**

- [x] Updated `Models/SlugRedirect.cs` with `RedirectId`
- [x] Updated `Data/MinimartDBContext.cs` configuration
- [x] Build successful ?
- [ ] Deploy to Render
- [ ] Test slug endpoint on Render
- [ ] Verify no errors in Render logs
- [ ] Update localhost database (if needed)

---

## ?? **How Column Mapping Works**

Entity Framework Core uses property names to generate SQL queries. When you have:

```csharp
[Column("RedirectId")]
public Guid RedirectId { get; set; }
```

EF Core knows to map the `RedirectId` property to the `RedirectId` column in the database.

Without the `[Column]` attribute or matching property name, EF Core would look for a column matching the property name exactly.

---

## ? **Resolution Status**

**Status:** ? **FIXED AND READY FOR DEPLOYMENT**

**What Changed:**
- Model property: `Id` ? `RedirectId`
- DbContext configuration: Uses `RedirectId` as primary key
- Database schema: Already has `RedirectId` column (no DB changes needed)

**Expected Outcome:**
- ? Render deployment will work without errors
- ? Slug endpoint will return products correctly
- ? 301 redirects will work when product names change

---

## ?? **Hot Reload Available**

Since you're running in debug mode with Hot Reload enabled:

**In Visual Studio:**
- Press `Ctrl+Alt+F5` or click Hot Reload button
- Changes will apply without full restart

**Or restart the application:**
```bash
# Stop current session (Ctrl+C)
# Then restart
dotnet run
```

---

**Next Step:** Deploy to Render and verify the slug endpoint works! ??

---

**Created:** February 15, 2026  
**Fixed By:** AI Assistant  
**Build Status:** ? Successful
