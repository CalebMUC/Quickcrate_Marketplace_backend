using System.Threading.Tasks;
using Minimart_Api.DTOS;
using Minimart_Api.Models;

namespace Minimart_Api.Services.OpenSearchService
{
    public interface IOpenSearchService
    {
        Task CreateIndexAsync(string indexname);
        Task IndexProductAsync(Product product);
        Task<IEnumerable<Product>> SearchProductsAsync(string query);
        Task<IEnumerable<string>> AutocompleteAsync(string query);
    }
}
