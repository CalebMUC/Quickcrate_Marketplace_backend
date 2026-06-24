namespace Minimart_Api.DTOS.Search
{
    public class SearchRequest
    {
        public string Q { get; set; } = "";

        // Category filters
        public string? CategoryId { get; set; }
        public string? SubCategoryId { get; set; }
        public string? SubSubCategoryId { get; set; }

        // Other filters
        public string? Brand { get; set; }
        public string? MerchantId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Spec filters from ProductSpecification parsing
        // e.g. { "panel_type": "ips", "refresh_rate": "75 hz", "ram": "16gb" }
        public Dictionary<string, string>? SpecFilters { get; set; }

        // Sorting: relevance | price_asc | price_desc | newest | rating
        public string SortBy { get; set; } = "relevance";

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 24;

        // Validated helpers
        public int SafePageSize => Math.Clamp(PageSize, 1, 48);
        public int Offset => (Math.Max(1, Page) - 1) * SafePageSize;
    }
}
