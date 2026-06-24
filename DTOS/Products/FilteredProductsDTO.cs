namespace Minimart_Api.DTOS.Products
{
    public class FilteredProductsDTO
    {
        public string SearchQuery { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int? CategoryID { get; set; }
        public int? SubCategoryID { get; set; }
        public Dictionary<string, string[]> Filters { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

}
