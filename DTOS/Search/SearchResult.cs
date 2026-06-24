namespace Minimart_Api.DTOS.Search
{
    public class SearchResult
    {
        public string productID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string KeyFeatures { get; set; } = string.Empty;
        public string Specification { get; set; } = string.Empty;
        public string Box { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? price { get; set; }
        public int Instock { get; set; }
    }
}
