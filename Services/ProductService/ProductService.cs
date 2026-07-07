using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Repositories.ProductRepository;
using Minimart_Api.Services.ProductService;
using Microsoft.Extensions.Logging;

namespace Minimart_Api.Services.ProductService
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepo, ILogger<ProductService> logger)
        {
            _productRepo = productRepo;
            _logger = logger;
        }

        // ==========================
        // BASIC CRUD
        // ==========================
        public async Task<ProductResponseDto?> GetByIdAsync(Guid productId)
        {
            return await _productRepo.GetByIdAsync(productId);
        }

        /// <summary>
        /// Get a specific product by ID with enhanced business logic and validation
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details or null if not found</returns>
        public async Task<ProductResponseDto?> GetProductAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting product details for ProductId: {ProductId}", productId);

                // Validate input
                if (productId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid ProductId provided: {ProductId}", productId);
                    throw new ArgumentException("Product ID cannot be empty", nameof(productId));
                }

                // Check if product exists first
                var productExists = await _productRepo.ProductExistsAsync(productId);
                if (!productExists)
                {
                    _logger.LogWarning("Product not found: {ProductId}", productId);
                    return null;
                }

                // Get the product details
                var product = await _productRepo.GetByIdAsync(productId);
                
                if (product == null)
                {
                    _logger.LogWarning("Product returned null despite existing: {ProductId}", productId);
                    return null;
                }

                // Additional business logic validation
                if (product.IsDeleted)
                {
                    _logger.LogWarning("Attempted to retrieve deleted product: {ProductId}", productId);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved product: {ProductId}", productId);
                return product;
            }
            catch (ArgumentException)
            {
                // Re-throw argument exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", productId);
                throw new InvalidOperationException($"An error occurred while retrieving product {productId}", ex);
            }
        }

        public async Task<PagedResultDto<ProductListDto>> GetAllAsync(ProductFilterDto filter)
        {
            return await _productRepo.GetAllAsync(filter);
        }

        public async Task<PagedResultDto<ProductListDto>> GetProductsByMerchantIdAsync(Guid merchantId, ProductFilterDto filter)
        {
            return await _productRepo.GetProductsByMerchantIdAsync(merchantId, filter);
        }

        public async Task<PagedResultDto<ProductListDto>> GetProductsByCategoryAsync(Guid categoryId, ProductFilterDto filter)
        {
            return await _productRepo.GetProductsByCategoryAsync(categoryId, filter);
        }
        public async Task<ProductPagedResultDto<ProductListDto>> GetFilteredProductsByCategoryAsync(
                Guid categoryId,
                ProductFilterDto filter,
                CancellationToken ct = default)
        {
            return await _productRepo.GetFilteredProductsByCategoryAsync(categoryId, filter, ct);
        }

        public async Task<ProductPagedResultDto<ProductListDto>> GetFilteredProductsBySubCategoryAsync(
               Guid subCategoryId,
               ProductFilterDto filter,
               CancellationToken ct = default)
        {
            return await _productRepo.GetFilteredProductsBySubCategoryAsync(subCategoryId, filter, ct);
        }

        //public async Task<PagedResultDto<ProductListDto>> GetProductsBySubCategoryAsync(Guid categoryId, ProductFilterDto filter)
        //{
        // return await _productRepo.GetProductsBySubCategoryAsync(categoryId, filter);
        //}
        public async Task<ProductResponseDto> CreateAsync(CreateProductDto createProductDto, string createdBy)
        {
            return await _productRepo.CreateAsync(createProductDto, createdBy);
        }

        public async Task<ProductResponseDto> UpdateAsync(UpdateProductDto updateProductDto, string updatedBy)
        {
            return await _productRepo.UpdateAsync(updateProductDto, updatedBy);
        }

        public async Task<bool> DeleteAsync(Guid productId, string deletedBy)
        {
            return await _productRepo.DeleteAsync(productId, deletedBy);
        }

        public async Task<bool> UpdateProductAsync(string productId, string status)
        {
            return await _productRepo.UpdateProductAsync(productId, status);
        }

        /// <summary>
        /// Approve or reject a product
        /// </summary>
        /// <param name="productId">The ID of the product</param>
        /// <param name="status">The new status of the product (e.g., "approved", "rejected")</param>
        /// <param name="approvedBy">The ID of the user approving/rejecting the product</param>
        /// <returns>True if the operation was successful, otherwise false</returns>
        public async Task<bool> ApproveProductAsync(string productId, string status, string approvedBy)
        {
            try
            {
                _logger.LogInformation("Approving product {ProductId} with status {Status}", productId, status);
                return await _productRepo.ApproveProductAsync(productId, status, approvedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving product {ProductId}", productId);
                throw;
            }
        }

        // ==========================
        // ADVANCED OPERATIONS
        // ==========================
        public async Task<bool> SoftDeleteAsync(Guid productId, string deletedBy)
        {
            return await _productRepo.SoftDeleteAsync(productId, deletedBy);
        }

        public async Task<bool> RestoreAsync(Guid productId, string restoredBy)
        {
            return await _productRepo.RestoreAsync(productId, restoredBy);
        }

        public async Task<PagedResultDto<ProductListDto>> GetDeletedProductsAsync(Guid merchantId, ProductFilterDto filter)
        {
            return await _productRepo.GetDeletedProductsAsync(merchantId, filter);
        }


        // ==========================
        // BULK OPERATIONS
        // ==========================
        public async Task<bool> BulkUpdateStatusAsync(BulkUpdateProductStatusDto bulkUpdateDto, string updatedBy)
        {
            return await _productRepo.BulkUpdateStatusAsync(bulkUpdateDto, updatedBy);
        }

        public async Task<bool> BulkDeleteAsync(BulkDeleteProductDto bulkDeleteDto, string deletedBy)
        {
            return await _productRepo.BulkDeleteAsync(bulkDeleteDto, deletedBy);
        }

        public async Task<bool> BulkSoftDeleteAsync(BulkDeleteProductDto bulkDeleteDto, string deletedBy)
        {
            return await _productRepo.BulkSoftDeleteAsync(bulkDeleteDto, deletedBy);
        }


        // ==========================
        // STATUS MANAGEMENT
        // ==========================
        public async Task<bool> UpdateStatusAsync(Guid productId, string status, string updatedBy)
        {
            return await _productRepo.UpdateStatusAsync(productId, status, updatedBy);
        }

        public async Task<bool> ToggleActiveStatusAsync(Guid productId, string updatedBy)
        {
            return await _productRepo.ToggleActiveStatusAsync(productId, updatedBy);
        }

        public async Task<bool> ToggleFeaturedStatusAsync(Guid productId, string updatedBy)
        {
            return await _productRepo.ToggleFeaturedStatusAsync(productId, updatedBy);
        }


        // ==========================
        // STOCK MANAGEMENT
        // ==========================
        public async Task<bool> UpdateStockAsync(Guid productId, int newStock, string updatedBy)
        {
            return await _productRepo.UpdateStockAsync(productId, newStock, updatedBy);
        }

        public async Task<bool> AdjustStockAsync(Guid productId, int adjustment, string updatedBy, string reason)
        {
            return await _productRepo.AdjustStockAsync(productId, adjustment, updatedBy, reason);
        }

        public async Task<List<ProductSummaryDto>> GetLowStockProductsAsync(Guid merchantId, int threshold = 10)
        {
            return await _productRepo.GetLowStockProductsAsync(merchantId, threshold);
        }

        public async Task<List<ProductSummaryDto>> GetOutOfStockProductsAsync(Guid merchantId)
        {
            return await _productRepo.GetOutOfStockProductsAsync(merchantId);
        }


        // ==========================
        // CATEGORY-BASED QUERIES
        // ==========================
        public async Task<PagedResultDto<ProductListDto>> GetSubCategoryProductsAsync(Guid subCategoryId, ProductFilterDto filter)
        {
            return await _productRepo.GetSubCategoryProductsAsync(subCategoryId, filter);
        }

        public async Task<PagedResultDto<ProductListDto>> GetProductsBySubSubCategoryAsync(Guid subSubCategoryId, ProductFilterDto filter)
        {
            return await _productRepo.GetProductsBySubSubCategoryAsync(subSubCategoryId, filter);
        }


        // ==========================
        // SEARCH & FILTER
        // ==========================
        public async Task<PagedResultDto<ProductListDto>> SearchProductsAsync(string searchTerm, Guid? merchantId, ProductFilterDto filter)
        {
            return await _productRepo.SearchProductsAsync(searchTerm, merchantId, filter);
        }

        /// <summary>
        /// Get featured products with optional merchant filtering
        /// </summary>
        /// <param name="merchantId">Optional merchant ID filter</param>
        /// <param name="count">Number of featured products to return</param>
        /// <returns>List of featured products</returns>
        public async Task<List<ProductListDto>> GetFeaturedProductsAsync(Guid? merchantId, int count = 10)
        {
            try
            {
                _logger.LogInformation("Getting featured products: MerchantId={MerchantId}, Count={Count}", merchantId, count);

                // Validate inputs
                if (count <= 0)
                {
                    throw new ArgumentException("Count must be greater than 0", nameof(count));
                }

                if (count > 100)
                {
                    throw new ArgumentException("Count cannot exceed 100", nameof(count));
                }

                // If merchantId is provided, use the merchant-specific method
                if (merchantId.HasValue)
                {
                    return await _productRepo.GetFeaturedProductsAsync(merchantId.Value, count);
                }

                // Otherwise, get all featured products using enhanced repository method
                return await _productRepo.GetFeaturedProductsAsync(count);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products: MerchantId={MerchantId}, Count={Count}", merchantId, count);
                throw new InvalidOperationException("An error occurred while retrieving featured products", ex);
            }
        }

        /// <summary>
        /// Get featured products with enhanced filtering options including category
        /// </summary>
        /// <param name="merchantId">Optional merchant ID filter</param>
        /// <param name="count">Number of featured products to return</param>
        /// <param name="categoryId">Optional category ID filter</param>
        /// <returns>List of featured products</returns>
        public async Task<List<ProductListDto>> GetFeaturedProductsAsync(Guid? merchantId, int count, Guid? categoryId)
        {
            try
            {
                _logger.LogInformation("Getting featured products with filters: MerchantId={MerchantId}, Count={Count}, CategoryId={CategoryId}", 
                    merchantId, count, categoryId);

                // Validate inputs
                if (count <= 0)
                {
                    throw new ArgumentException("Count must be greater than 0", nameof(count));
                }

                if (count > 100)
                {
                    throw new ArgumentException("Count cannot exceed 100", nameof(count));
                }

                // Use enhanced repository method that supports category filtering
                return await _productRepo.GetFeaturedProductsAsync(merchantId, count, categoryId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products with filters: MerchantId={MerchantId}, Count={Count}, CategoryId={CategoryId}", 
                    merchantId, count, categoryId);
                throw new InvalidOperationException("An error occurred while retrieving featured products", ex);
            }
        }

        public async Task<List<ProductSummaryDto>> GetRecentProductsAsync(Guid merchantId, int count = 10)
        {
            return await _productRepo.GetRecentProductsAsync(merchantId, count);
        }


        // ==========================
        // ANALYTICS
        // ==========================
        public async Task<ProductStatisticsDto> GetProductStatisticsAsync(Guid merchantId)
        {
            return await _productRepo.GetProductStatisticsAsync(merchantId);
        }

        public async Task<Dictionary<string, int>> GetProductCountByCategoryAsync(Guid merchantId)
        {
            return await _productRepo.GetProductCountByCategoryAsync(merchantId);
        }

        public async Task<Dictionary<string, decimal>> GetInventoryValueByCategoryAsync(Guid merchantId)
        {
            return await _productRepo.GetInventoryValueByCategoryAsync(merchantId);
        }


        // ==========================
        // VALIDATION
        // ==========================
        public async Task<bool> IsSkuUniqueAsync(string sku, Guid merchantId, Guid? excludeProductId = null)
        {
            return await _productRepo.IsSkuUniqueAsync(sku, merchantId, excludeProductId);
        }

        public async Task<bool> ProductExistsAsync(Guid productId)
        {
            return await _productRepo.ProductExistsAsync(productId);
        }

        public async Task<bool> ProductBelongsToMerchantAsync(Guid productId, Guid merchantId)
        {
            return await _productRepo.ProductBelongsToMerchantAsync(productId, merchantId);
        }


        // ==========================
        // IMPORT / EXPORT
        // ==========================
        public async Task<List<ProductResponseDto>> ImportProductsAsync(List<CreateProductDto> products, string createdBy)
        {
            return await _productRepo.ImportProductsAsync(products, createdBy);
        }

        public async Task<byte[]> ExportProductsAsync(Guid merchantId, ProductFilterDto filter)
        {
            return await _productRepo.ExportProductsAsync(merchantId, filter);
        }


        // ==========================
        // IMAGE MANAGEMENT
        // ==========================
        public async Task<bool> UpdateProductImagesAsync(Guid productId, List<string> imageUrls, string updatedBy)
        {
            return await _productRepo.UpdateProductImagesAsync(productId, imageUrls, updatedBy);
        }

        public async Task<bool> AddProductImageAsync(Guid productId, string imageUrl, string updatedBy)
        {
            return await _productRepo.AddProductImageAsync(productId, imageUrl, updatedBy);
        }

        public async Task<bool> RemoveProductImageAsync(Guid productId, string imageUrl, string updatedBy)
        {
            return await _productRepo.RemoveProductImageAsync(productId, imageUrl, updatedBy);
        }


        // ==========================
        // PRICING
        // ==========================
        public async Task<bool> UpdatePriceAsync(Guid productId, decimal newPrice, string updatedBy)
        {
            return await _productRepo.UpdatePriceAsync(productId, newPrice, updatedBy);
        }

        public async Task<bool> ApplyDiscountAsync(Guid productId, decimal discount, string updatedBy)
        {
            return await _productRepo.ApplyDiscountAsync(productId, discount, updatedBy);
        }

        public async Task<bool> BulkUpdatePricesAsync(List<Guid> productIds, decimal priceAdjustment, bool isPercentage, string updatedBy)
        {
            return await _productRepo.BulkUpdatePricesAsync(productIds, priceAdjustment, isPercentage, updatedBy);
        }


        // ==========================
        // DUPLICATE & COPY
        // ==========================
        public async Task<ProductResponseDto> DuplicateProductAsync(Guid productId, string newProductName, string createdBy)
        {
            return await _productRepo.DuplicateProductAsync(productId, newProductName, createdBy);
        }

        public async Task<List<ProductResponseDto>> CopyProductsToMerchantAsync(List<Guid> productIds, Guid targetMerchantId, string createdBy)
        {
            return await _productRepo.CopyProductsToMerchantAsync(productIds, targetMerchantId, createdBy);
        }

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
                return await _productRepo.GetProductBySlugAsync(slug);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product by slug: {Slug}", slug);
                throw;
            }
        }

        /// <summary>
        /// Update product slug (called when product name changes)
        /// </summary>
        public async Task<bool> UpdateProductSlugAsync(Guid productId, string newSlug, string updatedBy)
        {
            try
            {
                _logger.LogInformation("Updating slug for product {ProductId}", productId);
                return await _productRepo.UpdateProductSlugAsync(productId, newSlug, updatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slug for product {ProductId}", productId);
                throw;
            }
        }
    }
}
