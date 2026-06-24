namespace Minimart_Api.DTOS.Products
{
    public class ProductDto
    {
        public string ProductID { get; set; }
        public int merchantId { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public double DeliveryFee { get; set; }
    }
}
