namespace Minimart_Api.DTOS.Products
{
    // ProductFilterParams.cs (new class)
    public class ProductFilterParams
    {
        public string SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? SubCategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStock { get; set; } = false;
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
        public Dictionary<string, string[]> Features { get; set; } = new();
    }
}
