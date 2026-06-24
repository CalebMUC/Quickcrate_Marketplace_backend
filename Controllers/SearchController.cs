using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Products;
using Minimart_Api.DTOS.Search;
using Minimart_Api.Services.OpenSearchService;
using Minimart_Api.Services.SearchService.SearchService;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        //private readonly IOpenSearchService _openSearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger) { 
            _searchService = searchService;
            //_openSearchService = openSearchService;
            _logger = logger;
        }

        [HttpGet("ConvertToJson")]
        public async Task<IActionResult> ConvertToJson()
        {
            try
            {

                var Response = await _searchService.UpdateColumnJson();

                return Ok(Response);

            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        // POST: api/Search/CreateIndex
        //[HttpPost("CreateIndex")]
        //public async Task<IActionResult> CreateIndex([FromQuery] string indexName)
        //{
        //    try
        //    {
        //        await _openSearchService.CreateIndexAsync(indexName);
        //        return Ok($"Index '{indexName}' created successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Failed to create index: {ex.Message}");
        //    }
        //}

        // GET: api/Search/SearchProducts
        //[HttpGet("SearchProducts")]
        //public async Task<IActionResult> SearchProducts([FromQuery] string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //    {
        //        return BadRequest("Search query is required.");
        //    }

        //    try
        //    {
        //        var products = await _openSearchService.SearchProductsAsync(query);
        //        return Ok(products);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Search operation failed: {ex.Message}");
        //    }
        //}


        // GET: api/Search/Autocomplete
        //[HttpGet("Autocomplete")]
        //public async Task<IActionResult> Autocomplete([FromQuery] string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //    {
        //        return BadRequest("Query for autocomplete is required.");
        //    }

        //    try
        //    {
        //        var suggestions = await _openSearchService.AutocompleteAsync(query);
        //        return Ok(suggestions);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Autocomplete operation failed: {ex.Message}");
        //    }
        //}


        // GET: api/Search/SearchSuggestion
        [HttpGet("SearchSuggestion")]
        public async Task<IActionResult> SearchSuggestion([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query for autocomplete is required.");
            }

            try
            {
                //var suggestions = await  _searchService.GetSearchSuggestion(query);
                var suggestions = await _searchService.GetAutocompleteSuggestions(query);

                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Suggestions Failed");
                return BadRequest($"Autocomplete operation failed: {ex.Message}");
            }
        }

        // ── GET /api/search ──────────────────────────────────────────────────────
        // Full search with filters, sorting, pagination, and spec facets
        //
        // Examples:
        //   /api/search?q=lenovo+monitor
        //   /api/search?q=monitor&categoryId=xxx&brand=Lenovo&minPrice=10000
        //   /api/search?q=monitor&SpecFilters[panel_type]=ips&SpecFilters[refresh_rate]=75+hz
        //   /api/search?q=laptop&sortBy=price_asc&page=2&pageSize=24

        [HttpGet]
        [ProducesResponseType(typeof(SearchResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] SearchRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Q))
                return BadRequest(new { error = "Search query 'q' is required." });

            if (req.Q.Trim().Length < 1)
                return BadRequest(new { error = "Search query must be at least 1 character." });

            try
            {
                var results = await _searchService.SearchAsync(req);

                // Fire-and-forget analytics — never block the response for logging
                //_ = LogSearchSafeAsync(req, results.TotalCount);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search failed for query '{Q}'", req.Q);
                return StatusCode(500, new { error = "Search temporarily unavailable. Please try again." });
            }
        }

        [HttpGet("searchProducts")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return BadRequest("Search query cannot be empty");

                var results = await _searchService.SearchProductsAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }




        [HttpGet("GetSearchResults")]
        public async Task<IActionResult> GetSearchResults([FromQuery] string queryname)
        {
            try { 

                var Response = await _searchService.GetSearchResults(queryname);

                return Ok(Response);

            }catch (Exception ex)
            {
                //throw new Exception(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetSearchProducts/{subcategoryId}")]
        public async Task<IActionResult> GetSearchProducts(int categoryId)
        {
            var products = await _searchService.GetSearchProducts(categoryId);

            if (products == null || !products.Any())
            {
                return NotFound("No features found for this subcategory.");
            }

            return Ok(products);
        }

        // POST
        [HttpPost("GetFilteredProducts")]
        public async Task<IActionResult> GetFilteredProducts([FromBody] FilteredProductsDTO request)
        {
            try
            {
                // Validate request
                if (request == null)
                {
                    return BadRequest("Invalid request data");
                }

                // Build filter parameters - convert int IDs to Guid (or use null if conversion fails)
                var filterParams = new ProductFilterParams
                {
                    SearchTerm = request.SearchQuery,
                    CategoryId = null, // For now, set to null since we can't reliably map int to Guid
                    SubCategoryId = null, // For now, set to null since we can't reliably map int to Guid
                    PageNumber = request.Page,
                    PageSize = request.PageSize,
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
                    Features = request.Filters ?? new Dictionary<string, string[]>()
                };

                // Get filtered products
                var result = await _searchService.GetFilteredProducts(filterParams);

                // Return paginated response
                return Ok(new
                {
                    Data = result.Items,
                    Total = result.TotalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(result.TotalCount / (double)request.PageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching filtered products");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

    }
}
