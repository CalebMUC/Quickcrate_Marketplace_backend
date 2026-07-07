using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.DTOS.Search;
using Minimart_Api.Models;

namespace Minimart_Api.Services.SearchService.SearchService
{
    public interface ISearchService
    {
        Task<IEnumerable<string>> GetSearchSuggestion(string queryName,int limit = 10);

        Task<AutocompleteResponse> GetAutocompleteSuggestions(string? prefix);

        Task<SearchResponse> SearchAsync(SearchRequest request);


        Task<IEnumerable<GetProductsDto>> SearchProductsAsync(string queryName);

        Task<IEnumerable<Models.Category>> GetSearchResults(string queryname);

        Task<Status> UpdateColumnJson();

        Task<IEnumerable<CartResults>> GetSearchProducts(int CategoryID);

        Task<PaginatedResult<Product>> GetFilteredProducts(ProductFilterParams filterParams);


    }
}
