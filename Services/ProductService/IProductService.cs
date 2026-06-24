using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;

namespace Minimart_Api.Services.ProductService
{
    public interface IProductService
    {
        // Basic CRUD Operations
        Task<ProductResponseDto?> GetByIdAsync(Guid productId);
        
        /// <summary>
        /// Get a specific product by ID with enhanced error handling and business logic validation
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details or null if not found</returns>
        Task<ProductResponseDto?> GetProductAsync(Guid productId);
        
        Task<PagedResultDto<ProductListDto>> GetAllAsync(ProductFilterDto filter);
        Task<PagedResultDto<ProductListDto>> GetProductsByMerchantIdAsync(Guid merchantId, ProductFilterDto filter);
        Task<PagedResultDto<ProductListDto>> GetProductsByCategoryAsync(Guid categoryId, ProductFilterDto filter);
        Task<PagedResultDto<ProductListDto>> GetSubCategoryProductsAsync(Guid categoryId, ProductFilterDto filter);
        Task<ProductResponseDto> CreateAsync(CreateProductDto createProductDto, string createdBy);
        Task<ProductResponseDto> UpdateAsync(UpdateProductDto updateProductDto, string updatedBy);
        Task<bool> DeleteAsync(Guid productId, string deletedBy);

        Task<bool> UpdateProductAsync(string productId, string status);

        // Advanced Operations
        Task<bool> SoftDeleteAsync(Guid productId, string deletedBy);
        Task<bool> RestoreAsync(Guid productId, string restoredBy);
        Task<PagedResultDto<ProductListDto>> GetDeletedProductsAsync(Guid merchantId, ProductFilterDto filter);

        // Bulk Operations
        Task<bool> BulkUpdateStatusAsync(BulkUpdateProductStatusDto bulkUpdateDto, string updatedBy);
        Task<bool> BulkDeleteAsync(BulkDeleteProductDto bulkDeleteDto, string deletedBy);
        Task<bool> BulkSoftDeleteAsync(BulkDeleteProductDto bulkDeleteDto, string deletedBy);

        // Status Management
        Task<bool> UpdateStatusAsync(Guid productId, string status, string updatedBy);
        Task<bool> ToggleActiveStatusAsync(Guid productId, string updatedBy);
        Task<bool> ToggleFeaturedStatusAsync(Guid productId, string updatedBy);

        // Stock Management
        Task<bool> UpdateStockAsync(Guid productId, int newStock, string updatedBy);
        Task<bool> AdjustStockAsync(Guid productId, int adjustment, string updatedBy, string reason);
        Task<List<ProductSummaryDto>> GetLowStockProductsAsync(Guid merchantId, int threshold = 10);
        Task<List<ProductSummaryDto>> GetOutOfStockProductsAsync(Guid merchantId);

        // Category-based Queries
        //Task<PagedResultDto<ProductListDto>> GetProductsBySubCategoryAsync(Guid subCategoryId, ProductFilterDto filter);
        Task<PagedResultDto<ProductListDto>> GetProductsBySubSubCategoryAsync(Guid subSubCategoryId, ProductFilterDto filter);

        // Search and Filtering
        Task<PagedResultDto<ProductListDto>> SearchProductsAsync(string searchTerm, Guid? merchantId, ProductFilterDto filter);
        
        /// <summary>
        /// Get featured products with optional filtering
        /// </summary>
        /// <param name="merchantId">Optional merchant ID filter</param>
        /// <param name="count">Number of featured products to return</param>
        /// <returns>List of featured products</returns>
        Task<List<ProductListDto>> GetFeaturedProductsAsync(Guid? merchantId, int count = 10);
        
        /// <summary>
        /// Get featured products with enhanced filtering options
        /// </summary>
        /// <param name="merchantId">Optional merchant ID filter</param>
        /// <param name="count">Number of featured products to return</param>
        /// <param name="categoryId">Optional category ID filter</param>
        /// <returns>List of featured products</returns>
        Task<List<ProductListDto>> GetFeaturedProductsAsync(Guid? merchantId, int count, Guid? categoryId);
        
        Task<List<ProductSummaryDto>> GetRecentProductsAsync(Guid merchantId, int count = 10);

        // Analytics and Statistics
        Task<ProductStatisticsDto> GetProductStatisticsAsync(Guid merchantId);
        Task<Dictionary<string, int>> GetProductCountByCategoryAsync(Guid merchantId);
        Task<Dictionary<string, decimal>> GetInventoryValueByCategoryAsync(Guid merchantId);

        // Validation and Business Logic
        Task<bool> IsSkuUniqueAsync(string sku, Guid merchantId, Guid? excludeProductId = null);
        Task<bool> ProductExistsAsync(Guid productId);
        Task<bool> ProductBelongsToMerchantAsync(Guid productId, Guid merchantId);

        // Import/Export
        Task<List<ProductResponseDto>> ImportProductsAsync(List<CreateProductDto> products, string createdBy);
        Task<byte[]> ExportProductsAsync(Guid merchantId, ProductFilterDto filter);

        // Image Management
        Task<bool> UpdateProductImagesAsync(Guid productId, List<string> imageUrls, string updatedBy);
        Task<bool> AddProductImageAsync(Guid productId, string imageUrl, string updatedBy);
        Task<bool> RemoveProductImageAsync(Guid productId, string imageUrl, string updatedBy);

        // Pricing
        Task<bool> UpdatePriceAsync(Guid productId, decimal newPrice, string updatedBy);
        Task<bool> ApplyDiscountAsync(Guid productId, decimal discount, string updatedBy);
        Task<bool> BulkUpdatePricesAsync(List<Guid> productIds, decimal priceAdjustment, bool isPercentage, string updatedBy);

        // ==========================
        // DUPLICATE & COPY
        // ==========================
        Task<ProductResponseDto> DuplicateProductAsync(Guid productId, string newProductName, string createdBy);
        Task<List<ProductResponseDto>> CopyProductsToMerchantAsync(List<Guid> productIds, Guid targetMerchantId, string createdBy);

        // ==========================
        // SEO SLUG MANAGEMENT
        // ==========================
        Task<ProductResponseDto?> GetProductBySlugAsync(string slug);
        Task<bool> UpdateProductSlugAsync(Guid productId, string newSlug, string updatedBy);
        
        // Product Approval
        Task<bool> ApproveProductAsync(string productId, string status, string approvedBy);
    }
}
