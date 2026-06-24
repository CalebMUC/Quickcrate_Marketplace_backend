using Minimart_Api.DTOS.Cart;
using StackExchange.Redis;

namespace Minimart_Api.Services.Recommedation
{
    public interface IRecomedationService
    {
        Task<IEnumerable<SavedProductsDto>> GetPersonalizedRecommendations(string userId, int limit = 5);
        Task<IEnumerable<SavedProductsDto>> GetComplementaryProducts(string productId, int limit = 5);
        Task<IEnumerable<SavedProductsDto>> GetFrequentlyBoughtTogether(string productId, int limit = 5);
    }
}
