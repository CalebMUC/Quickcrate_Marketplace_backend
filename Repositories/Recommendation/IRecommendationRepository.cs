using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Recommendation
{
    public interface IRecommendationRepository
    {
        Task<IEnumerable<Models.Order>> GetUserOrders(string userId);
        Task<IEnumerable<Product>> GetPopularProductsByCategory(Guid? categoryId, int limit);
        Task<IEnumerable<Product>> GetPopularProducts(int limit);
        Task<IEnumerable<Models.Order>> GetOrdersContainingProduct(Guid productId);
        Task<IEnumerable<Product>> GetProductsByCategory(Guid? categoryId, int limit);
    }
}
