using Microsoft.Extensions.Caching.Memory;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.DTOS.Search;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Search;
using Minimart_Api.Services.SearchService.SearchService;

namespace Minimart_Api.Services.SearchService
{
    public class SearchServices : ISearchService
    {
        private readonly ISearchRepo _searchRepo;
        private readonly ILogger<SearchServices> _logger;
        private readonly IMemoryCache _memoryCache;

        public SearchServices(ISearchRepo searchRepo, ILogger<SearchServices> logger, IMemoryCache memoryCache)
        {
            _searchRepo = searchRepo;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task<AutocompleteResponse> GetAutocompleteSuggestions(string prefix) {

            return await _searchRepo.GetAutocompleteSuggestions(prefix);
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
           
            return await _searchRepo.SearchAsync(request);
        }

       
        public async Task<IEnumerable<string>> GetSearchSuggestion(string queryName, int limit = 10)
        {
            try
            {
                // Check cache first
                var cacheKey = $"search_suggestions_{queryName}_{limit}";
                
                if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<string> cachedSuggestions))
                {
                    return cachedSuggestions;
                }

                var suggestions = await _searchRepo.GetSearchSuggestion(queryName, limit);

                // Cache for 5 minutes
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                _memoryCache.Set(cacheKey, suggestions, cacheEntryOptions);

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for: {QueryName}", queryName);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<GetProductsDto>> SearchProductsAsync(string queryName)
        {
            try
            {
                return await _searchRepo.SearchProductsAsync(queryName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products for: {QueryName}", queryName);
                return Enumerable.Empty<GetProductsDto>();
            }
        }

        public async Task<IEnumerable<Models.Category>> GetSearchResults(string queryname)
        {
            try
            {
                return await _searchRepo.GetSearchResults(queryname);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search results for: {QueryName}", queryname);
                return Enumerable.Empty<Models.Category>();
            }
        }

        public async Task<Status> UpdateColumnJson()
        {
            try
            {
                return await _searchRepo.UpdateColumnJson();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating column JSON");
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Error updating column JSON"
                };
            }
        }

        public async Task<IEnumerable<CartResults>> GetSearchProducts(int CategoryID)
        {
            try
            {
                return await _searchRepo.GetSearchProducts(CategoryID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search products for category: {CategoryId}", CategoryID);
                return Enumerable.Empty<CartResults>();
            }
        }

        public async Task<PaginatedResult<Product>> GetFilteredProducts(ProductFilterParams filterParams)
        {
            try
            {
                var result = await _searchRepo.GetFilteredProducts(filterParams);
                return new PaginatedResult<Product>
                {
                    Items = result.Items,
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered products");
                return new PaginatedResult<Product>
                {
                    Items = new List<Product>(),
                    TotalCount = 0,
                    PageNumber = filterParams.PageNumber,
                    PageSize = filterParams.PageSize,
                    TotalPages = 0
                };
            }
        }
    }
}
