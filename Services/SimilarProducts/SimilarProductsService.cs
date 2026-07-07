using Minimart_Api.DTOS.Products;
using Minimart_Api.Repositories.ProductRepository;
using Minimart_Api.Models;

namespace Minimart_Api.Services.SimilarProducts
{
    public class SimilarProductsService : ISimilarProductsService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<SimilarProductsService> _logger;

        public SimilarProductsService(
            IProductRepository productRepository,
            ILogger<SimilarProductsService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<SimilarProductDto>> GetSimilarProductsAsync(string productId, int limit = 5)
        {
            try
            {
                // Parse productId to Guid
                if (!Guid.TryParse(productId, out Guid productGuid))
                {
                    _logger.LogWarning("Invalid product ID format: {ProductId}", productId);
                    return Enumerable.Empty<SimilarProductDto>();
                }

                var targetProduct = await _productRepository.GetByIdAsync(productGuid);
                if (targetProduct == null)
                {
                    _logger.LogWarning("Product {ProductId} not found", productId);
                    return Enumerable.Empty<SimilarProductDto>();
                }

                var similarProducts = await FindSimilarProducts(targetProduct, limit);
                var filteredProducts = FilterBySimilarityScore(similarProducts, targetProduct, 50);
                return filteredProducts.Take(limit);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar products for {ProductId}", productId);
                throw;
            }
        }

        private IEnumerable<SimilarProductDto> FilterBySimilarityScore(
            IEnumerable<ProductResponseDto> products,
            ProductResponseDto targetProduct,
            int minScore)
        {
            return products
                .Select(p => new SimilarProductDto
                {
                    ProductId = p.ProductId.ToString(),
                    ProductName = p.ProductName,
                    ImageUrl = p.ImageUrls?.FirstOrDefault() ?? "",
                    Price = p.Price,
                    Discount = (double)p.Discount,
                    InStock = p.InStock,
                    CategoryName = p.CategoryName,
                    ProductDescription = p.ProductDescription,
                    SimilarityScore = CalculateSimilarityScore(targetProduct, p)
                })
                .Where(p => p.SimilarityScore >= minScore)
                .OrderByDescending(p => p.SimilarityScore);
        }

        private async Task<IEnumerable<ProductResponseDto>> FindSimilarProducts(ProductResponseDto targetProduct, int limit)
        {
            var results = new List<ProductResponseDto>();

            try
            {
                // 1. Same category products using new repository method
                var categoryFilter = new ProductFilterDto
                {
                    //CategoryId = targetProduct.CategoryId,
                    IsActive = true,
                    PageSize = limit * 2, // Get more to filter out current product
                    Page = 1
                };

                var sameCategoryProducts = await _productRepository.GetProductsByCategoryAsync(
                    targetProduct.CategoryId, 
                    categoryFilter);

                // Convert ProductListDto to ProductResponseDto and exclude current product
                foreach (var product in sameCategoryProducts.Data.Where(p => p.ProductId != targetProduct.ProductId))
                {
                    var fullProduct = await _productRepository.GetByIdAsync(product.ProductId);
                    if (fullProduct != null)
                    {
                        results.Add(fullProduct);
                        if (results.Count >= limit) break;
                    }
                }

                // 2. Same sub-category products if we need more
                if (results.Count < limit && targetProduct.SubCategoryId.HasValue)
                {
                    var subCategoryFilter = new ProductFilterDto
                    {
                        //SubCategoryId = targetProduct.SubCategoryId.Value,
                        IsActive = true,
                        PageSize = limit - results.Count,
                        Page = 1
                    };

                    var sameSubCategoryProducts = await _productRepository.GetSubCategoryProductsAsync(
                        targetProduct.SubCategoryId.Value,
                        subCategoryFilter);

                    foreach (var product in sameSubCategoryProducts.Data.Where(p => p.ProductId != targetProduct.ProductId))
                    {
                        if (results.Any(r => r.ProductId == product.ProductId))
                            continue; // Skip duplicates

                        var fullProduct = await _productRepository.GetByIdAsync(product.ProductId);
                        if (fullProduct != null)
                        {
                            results.Add(fullProduct);
                            if (results.Count >= limit) break;
                        }
                    }
                }

                // 3. Keyword/search matching if we still need more
                if (results.Count < limit && !string.IsNullOrEmpty(targetProduct.ProductName))
                {
                    var keywords = targetProduct.ProductName.Split(' ')
                        .Where(k => k.Length > 3)
                        .Take(3); // Use top 3 meaningful words

                    foreach (var keyword in keywords)
                    {
                        if (results.Count >= limit) break;

                        var searchFilter = new ProductFilterDto
                        {
                            //ProductName = keyword,
                            IsActive = true,
                            PageSize = limit - results.Count,
                            Page = 1
                        };

                        var searchResults = await _productRepository.SearchProductsAsync(
                            keyword,
                            null, // Search across all merchants
                            searchFilter);

                        foreach (var product in searchResults.Data.Where(p => p.ProductId != targetProduct.ProductId))
                        {
                            if (results.Any(r => r.ProductId == product.ProductId))
                                continue; // Skip duplicates

                            var fullProduct = await _productRepository.GetByIdAsync(product.ProductId);
                            if (fullProduct != null)
                            {
                                results.Add(fullProduct);
                                if (results.Count >= limit) break;
                            }
                        }
                    }
                }

                // 4. Featured products as fallback
                if (results.Count < limit)
                {
                    var featuredProducts = await _productRepository.GetFeaturedProductsAsync(
                        null, // All merchants
                        limit - results.Count,
                        targetProduct.CategoryId); // Prefer same category

                    foreach (var product in featuredProducts.Where(p => p.ProductId != targetProduct.ProductId))
                    {
                        if (results.Any(r => r.ProductId == product.ProductId))
                            continue; // Skip duplicates

                        var fullProduct = await _productRepository.GetByIdAsync(product.ProductId);
                        if (fullProduct != null)
                        {
                            results.Add(fullProduct);
                            if (results.Count >= limit) break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar products for {ProductId}", targetProduct.ProductId);
            }

            return results.Take(limit);
        }

        private IEnumerable<SimilarProductDto> MapToDto(IEnumerable<ProductResponseDto> products, ProductResponseDto targetProduct)
        {
            return products.Select(p => new SimilarProductDto
            {
                ProductId = p.ProductId.ToString(),
                ProductName = p.ProductName,
                ImageUrl = p.ImageUrls?.FirstOrDefault() ?? "",
                Price = p.Price,
                Discount = (double)p.Discount,
                InStock = p.InStock,
                CategoryName = p.CategoryName,
                ProductDescription = p.ProductDescription,
                SimilarityScore = CalculateSimilarityScore(targetProduct, p)
            });
        }

        private double CalculateSimilarityScore(ProductResponseDto product1, ProductResponseDto product2)
        {
            double score = 0;

            // Category match (50% weight)
            if (product1.CategoryId == product2.CategoryId) 
                score += 50;
            // Sub-category match (30% weight)
            else if (product1.SubCategoryId.HasValue && 
                     product2.SubCategoryId.HasValue && 
                     product1.SubCategoryId == product2.SubCategoryId) 
                score += 30;

            // Price similarity (20% weight)
            var priceDiff = Math.Abs((double)(product1.Price - product2.Price));
            var maxPrice = Math.Max((double)product1.Price, (double)product2.Price);
            if (maxPrice > 0)
            {
                score += 20 * (1 - Math.Min(priceDiff / maxPrice, 1));
            }

            // Merchant match bonus (10% weight)
            if (product1.MerchantID == product2.MerchantID)
                score += 10;

            return Math.Round(score, 2);
        }
    }
}
