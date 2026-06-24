using Minimart_Api.DTOS.Products;

namespace Minimart_Api.Services.SimilarProducts
{
    public interface ISimilarProductsService
    {
        Task<IEnumerable<SimilarProductDto>> GetSimilarProductsAsync(string productId, int limit = 5);
    }
}
