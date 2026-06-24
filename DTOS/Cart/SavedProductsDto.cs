namespace Minimart_Api.DTOS.Cart
{
    public class SavedProductsDto
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public double Discount { get; set; }
        public bool InStock { get; set; } 
        public string CategoryName { get; set; }
        public DateTime SavedOn { get; set; }
        public int Quantity { get; set; }
    }
}
