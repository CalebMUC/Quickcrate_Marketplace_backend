using Minimart_Api.DTOS;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.DTOS.Search;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Search
{
    public interface ISearchRepo
    {
        Task<IEnumerable<string>> GetSearchSuggestion(string queryName,int limit=10);

        Task<AutocompleteResponse> GetAutocompleteSuggestions(string prefix);

        Task LogSearchAsync(SearchLog log);
        Task LogClickAsync(ClickEvent evt);
        Task<SearchResponse> SearchAsync(SearchRequest request);
        Task<IEnumerable<GetProductsDto>> SearchProductsAsync(string queryName);
        Task<IEnumerable<Models.Category>> GetSearchResults(string queryname);

        Task<Status> UpdateColumnJson();
        Task<IEnumerable<CartResults>> GetSearchProducts(int CategoryID);

        Task<PaginatedResult<Product>> GetFilteredProducts(ProductFilterParams filterParams);
    }
}
