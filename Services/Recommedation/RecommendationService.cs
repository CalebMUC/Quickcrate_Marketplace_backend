using Microsoft.Extensions.Caching.Memory;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Recommendation;
using Minimart_Api.Services.Recommedation;

namespace Minimart_Api.Services.Recommedation
{
    public class RecommendationService : IRecomedationService
    {
        private readonly IRecommendationRepository _recommendationRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(
            IRecommendationRepository recommendationRepository,
            IMemoryCache cache,
            ILogger<RecommendationService> logger)
        {
            _recommendationRepository = recommendationRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IEnumerable<SavedProductsDto>> GetPersonalizedRecommendations(string userId, int limit = 5)
        {
            try
            {
                var cacheKey = $"recommendations_user_{userId}_{limit}";
                
                if (_cache.TryGetValue(cacheKey, out IEnumerable<SavedProductsDto> cachedRecommendations))
                {
                    return cachedRecommendations;
                }

                var recommendations = new List<SavedProductsDto>();

                // Get user's order history
                var userOrders = await _recommendationRepository.GetUserOrders(userId);
                
                if (userOrders.Any())
                {
                    // Get recommendations based on purchase history
                    recommendations.AddRange(await GetPurchaseBasedRecommendations(userOrders, limit));
                }
                
                // If we don't have enough recommendations, add popular products
                if (recommendations.Count < limit)
                {
                    var popularProducts = await _recommendationRepository.GetPopularProducts(limit - recommendations.Count);
                    recommendations.AddRange(MapToSavedProductsDto(popularProducts));
                }

                // Cache for 1 hour
                _cache.Set(cacheKey, recommendations, TimeSpan.FromHours(1));

                return recommendations.Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user {UserId}", userId);
                return Enumerable.Empty<SavedProductsDto>();
            }
        }

        public async Task<IEnumerable<SavedProductsDto>> GetComplementaryProducts(string productId, int limit = 5)
        {
            try
            {
                if (!Guid.TryParse(productId, out Guid guid))
                {
                    return Enumerable.Empty<SavedProductsDto>();
                }

                var cacheKey = $"complementary_products_{productId}_{limit}";
                
                if (_cache.TryGetValue(cacheKey, out IEnumerable<SavedProductsDto> cachedProducts))
                {
                    return cachedProducts;
                }

                // Get orders containing this product
                var relatedOrders = await _recommendationRepository.GetOrdersContainingProduct(guid);
                var recommendations = new List<SavedProductsDto>();

                // For now, return category-based recommendations as complementary products
                var popularProducts = await _recommendationRepository.GetPopularProducts(limit);
                recommendations.AddRange(MapToSavedProductsDto(popularProducts));

                // Cache for 2 hours
                _cache.Set(cacheKey, recommendations, TimeSpan.FromHours(2));

                return recommendations.Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complementary products for {ProductId}", productId);
                return Enumerable.Empty<SavedProductsDto>();
            }
        }

        public async Task<IEnumerable<SavedProductsDto>> GetFrequentlyBoughtTogether(string productId, int limit = 5)
        {
            try
            {
                if (!Guid.TryParse(productId, out Guid guid))
                {
                    return Enumerable.Empty<SavedProductsDto>();
                }

                var cacheKey = $"frequently_bought_together_{productId}_{limit}";
                
                if (_cache.TryGetValue(cacheKey, out IEnumerable<SavedProductsDto> cachedProducts))
                {
                    return cachedProducts;
                }

                var recommendations = new List<SavedProductsDto>();

                // Get orders containing this product
                var relatedOrders = await _recommendationRepository.GetOrdersContainingProduct(guid);
                
                // For now, return popular products as frequently bought together
                var popularProducts = await _recommendationRepository.GetPopularProducts(limit);
                recommendations.AddRange(MapToSavedProductsDto(popularProducts));

                // Cache for 2 hours
                _cache.Set(cacheKey, recommendations, TimeSpan.FromHours(2));

                return recommendations.Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting frequently bought together for {ProductId}", productId);
                return Enumerable.Empty<SavedProductsDto>();
            }
        }

        private async Task<IEnumerable<SavedProductsDto>> GetPurchaseBasedRecommendations(
            IEnumerable<Models.Order> userOrders, int limit)
        {
            var recommendations = new List<SavedProductsDto>();

            // For now, get popular products as purchase-based recommendations
            var popularProducts = await _recommendationRepository.GetPopularProducts(limit);
            recommendations.AddRange(MapToSavedProductsDto(popularProducts));

            return recommendations;
        }

        private IEnumerable<SavedProductsDto> MapToSavedProductsDto(IEnumerable<Product> products)
        {
            return products.Select(p => new SavedProductsDto
            {
                ProductID = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Discount = (double)p.Discount,
                ProductImage = p.ImageUrls?.FirstOrDefault() ?? "",
                CategoryName = p.CategoryName,
                InStock = p.IsActive && p.StockQuantity > 0,
                SavedOn = DateTime.UtcNow, // Default value
                Quantity = 1 // Default value
            });
        }
    }
}
