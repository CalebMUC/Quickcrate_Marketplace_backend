using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Minimart_Api.DTOS.Products
{
    /// <summary>
    /// Base Product DTO - contains common properties shared across all product DTOs
    /// </summary>
    public abstract class BaseProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(255, ErrorMessage = "Product name cannot exceed 255 characters")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Product description cannot exceed 2000 characters")]
        public string ProductDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal Discount { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [StringLength(100, ErrorMessage = "SKU cannot exceed 100 characters")]
        public string SKU { get; set; } = string.Empty;

        // Category Information
        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryId { get; set; }

        [StringLength(255, ErrorMessage = "Category name cannot exceed 255 characters")]
        public string CategoryName { get; set; } = string.Empty;

        public Guid? SubCategoryId { get; set; }

        [StringLength(255, ErrorMessage = "SubCategory name cannot exceed 255 characters")]
        public string? SubCategoryName { get; set; }

        public Guid? SubSubCategoryId { get; set; }

        [StringLength(255, ErrorMessage = "SubSubCategory name cannot exceed 255 characters")]
        public string? SubSubCategoryName { get; set; }

        [StringLength(4000, ErrorMessage = "Product specification cannot exceed 4000 characters")]
        public string ProductSpecification { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Features cannot exceed 2000 characters")]
        public string Features { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Box contents cannot exceed 1000 characters")]
        public string BoxContents { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Product type cannot exceed 100 characters")]
        public string ProductType { get; set; } = string.Empty;

        // **SEO PROPERTIES - NEW**
        public string? Slug { get; set; }
        public DateTime? SlugUpdatedAt { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = "pending";

        public List<string> ImageUrls { get; set; } = new();

        // Merchant Information
        [Required(ErrorMessage = "Merchant ID is required")]
        public Guid MerchantID { get; set; }

        // Timestamps
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Calculated Properties
        public decimal DiscountedPrice => Price - (Price * Discount / 100);
        public bool InStock => IsActive && StockQuantity > 0;
    }

    // Create Product DTO
    public class CreateProductDto : BaseProductDto
    {
        [Required(ErrorMessage = "Merchant ID is required")]
        public Guid MerchantID { get; set; }
    }

    // Update Product DTO
    public class UpdateProductDto : BaseProductDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }
    }


    // Product Response DTO
    public class ProductResponseDto : BaseProductDto
    {
        public Guid ProductId { get; set; }
        public Guid MerchantID { get; set; }

        // Audit Fields
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }

        // Related entities
        public ProductMerchantDto? Merchant { get; set; }
        public ProductCategoryDto? Category { get; set; }
        public ProductSubCategoryDto? SubCategory { get; set; }
        public ProductSubSubCategoryDto? SubSubCategory { get; set; }
    }

    // Product List DTO - for listing/grid views
    public class ProductListDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? Description { get; set; }
        public string Slug { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int StockQuantity { get; set; }
        //public string[]? ImageUrls { get; set; }   // ← array, not string
        public List<string> ImageUrls { get; set; } = new();
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public string Status { get; set; } = "";
        public string? Brand { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public string? SubSubCategoryName { get; set; }
        public Guid MerchantID { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

        public string ProductSpecification { get; set; }
        public string ProductDescription { get; set; }
        public string Features { get; set; }
        public string BoxContents { get; set; }
        public string SKU { get; set; }

        // Computed — lets the frontend skip the calculation
        public decimal EffectivePrice => Price - Discount;
        public bool IsInStock => StockQuantity > 0;
        public bool HasDiscount => Discount > 0;
        public int DiscountPercent => HasDiscount
                                            ? (int)Math.Round((Discount / Price) * 100)
                                            : 0;
    }

    // Product Summary DTO - for dashboard/analytics
    public class ProductSummaryDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    // Product Filter DTO
    //public class ProductFilterDto
    //{
    //    public Guid? MerchantId { get; set; }
    //    public Guid? CategoryId { get; set; }
    //    public Guid? SubCategoryId { get; set; }
    //    public Guid? SubSubCategoryId { get; set; }
    //    public string? ProductName { get; set; }
    //    public string? SKU { get; set; }
    //    public decimal? MinPrice { get; set; }
    //    public decimal? MaxPrice { get; set; }
    //    public bool? IsActive { get; set; }
    //    public bool? IsFeatured { get; set; }
    //    public string? Status { get; set; }
    //    public string? ProductType { get; set; }
    //    public DateTime? CreatedFrom { get; set; }
    //    public DateTime? CreatedTo { get; set; }
    //    public int? MinStock { get; set; }
    //    public int? MaxStock { get; set; }

    //    // Pagination
    //    public int Page { get; set; } = 1;
    //    public int PageSize { get; set; } = 10;

    //    // Sorting
    //    public string SortBy { get; set; } = "CreatedOn";
    //    public string SortDirection { get; set; } = "DESC"; // ASC or DESC
    //}

    //public class ProductFilterDto
    //{
    //    public int Page { get; set; } = 1;
    //    public int PageSize { get; set; } = 24;
    //    public bool? IsActive { get; set; } = true;
    //    public string? Status { get; set; } = "approved";
    //    public bool? IsFeatured { get; set; }

    //    // ADD these — frontend sends them, backend ignores them currently
    //    public string? SortBy { get; set; } = "CreatedOn";    // CreatedOn | Price | Discount | ProductName
    //    public string? SortDirection { get; set; } = "DESC";         // ASC | DESC
    //    public string? SearchTerm { get; set; }
    //    public decimal? MinPrice { get; set; }
    //    public decimal? MaxPrice { get; set; }
    //    public string? Brand { get; set; }
    //}


    // Application/DTOs/ProductFilterDto.cs
    //public sealed class ProductFilterDto
    //{
    //    // Pagination
    //    public int Page { get; init; } = 1;
    //    public int PageSize { get; init; } = 20;

    //    // Status
    //    public string? Status { get; init; } = "approved";
    //    public bool IsActive { get; init; } = true;

    //    // Filters
    //    public List<string> Brands { get; init; } = [];
    //    public Guid? SubCategoryId { get; init; }
    //    public decimal? MinPrice { get; init; }
    //    public decimal? MaxPrice { get; init; }
    //    public int? MinDiscount { get; init; }   // e.g. 20 = 20%
    //    public bool InStockOnly { get; init; } = false;
    //    public string? SearchTerm { get; init; }
    //    public bool? IsFeatured { get; init; }

    //    // Sorting
    //    public string SortBy { get; init; } = "CreatedOn";
    //    public string SortDirection { get; init; } = "DESC";

    //    // Computed helpers (not serialised — set in the service)
    //    [JsonIgnore]
    //    public bool SortDescending =>
    //        string.Equals(SortDirection, "DESC", StringComparison.OrdinalIgnoreCase);
    //}

        
    public sealed class ProductFilterDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Status
        public string? Status { get; set; } = "approved";
        public bool IsActive { get; set; } = true;

        // Filters
        public List<string> Brands { get; set; } = [];
        public Guid? SubCategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinDiscount { get; set; }
        public bool InStockOnly { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsFeatured { get; set; }

        // Sorting
        public string SortBy { get; set; } = "CreatedOn";
        public string SortDirection { get; set; } = "DESC";

        [JsonIgnore]
        public bool SortDescending =>
            string.Equals(SortDirection, "DESC", StringComparison.OrdinalIgnoreCase);
    }
    // Supporting DTOs
    public class ProductMerchantDto
    {
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ProductCategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class ProductSubCategoryDto
    {
        public Guid SubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class ProductSubSubCategoryDto
    {
        public Guid SubSubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    // Bulk Operations DTOs
    public class BulkUpdateProductStatusDto
    {
        [Required]
        public List<Guid> ProductIds { get; set; } = new();

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }

    public class BulkDeleteProductDto
    {
        [Required]
        public List<Guid> ProductIds { get; set; } = new();
    }

    // Product Statistics DTO
    public class ProductStatisticsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int FeaturedProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public Dictionary<string, int> ProductsByStatus { get; set; } = new();
        public Dictionary<string, int> ProductsByCategory { get; set; } = new();
    }
}
