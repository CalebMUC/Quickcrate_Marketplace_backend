namespace Minimart_Api.DTOS.Orders
{
    public class OrderProductsDTO
    {
        public Guid ProductID { get; set; }
        public Guid merchantId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public double DeliveryFee { get; set; }

        public double Discount { get; set; }
        public string ImageUrl { get; set; }


    }
}
