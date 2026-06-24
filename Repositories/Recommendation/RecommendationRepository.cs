using Amazon.Runtime.Internal.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Minimart_Api.Data;
using Minimart_Api.Models;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.Products;
using OpenSearch.Client;

namespace Minimart_Api.Repositories.Recommendation
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly MinimartDBContext _dbContext;
        private readonly IMemoryCache _cache;
        public RecommendationRepository(MinimartDBContext dBContext,IMemoryCache cache) {
            _dbContext = dBContext;
            _cache = cache;
        }

        public async Task<IEnumerable<Models.Order>> GetUserOrders(string userId)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderProducts)
                    //.ThenInclude(op => op.Product) // Comment out if this navigation doesn't exist
                .Where(o => o.ApplicationUserId == userId) // Updated to use ApplicationUserId
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetPopularProductsByCategory(Guid? categoryId, int limit)
        {
            return await _dbContext.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.OrderItems) // Assuming OrderItems represents products in orders
                .OrderByDescending(p => p.OrderItems.Count)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetPopularProducts(int limit)
        {
            return await _dbContext.Products
                .Where(p => p.IsActive)
                .Include(p => p.OrderItems)
                .OrderByDescending(p => p.OrderItems.Count)
                .ThenBy(p => EF.Functions.Random()) // For variety if counts are equal
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Order>> GetOrdersContainingProduct(Guid productId)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderProducts)
                    //.ThenInclude(op => op.Product) // Comment out if this navigation doesn't exist
                .Where(o => o.OrderProducts.Any(op => op.ProductId == productId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategory(Guid? categoryId, int limit)
        {
            if (!categoryId.HasValue)
            {
                return await GetPopularProducts(limit);
            }

            var cacheKey = $"category_products_{categoryId}_{limit}";

            if (_cache.TryGetValue(cacheKey, out IEnumerable<Product> cachedProducts))
            {
                return cachedProducts;
            }

            var products = await _dbContext.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.OrderItems)
                .OrderByDescending(p => p.OrderItems.Count)
                .ThenBy(p => Guid.NewGuid())
                .Take(limit)
                .ToListAsync();

            // Cache for 1 hour
            _cache.Set(cacheKey, products, TimeSpan.FromHours(1));

            return products;
        }
    }
}
