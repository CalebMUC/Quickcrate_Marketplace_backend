namespace Minimart_Api.DTOS.Products
{
    public class SimilarProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public double Discount { get; set; }
        public bool InStock { get; set; }
        public string CategoryName { get; set; }

        public string ProductDescription { get; set; }
        public double SimilarityScore { get; set; }
    }
}
