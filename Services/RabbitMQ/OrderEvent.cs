using Minimart_Api.DTOS.Products;

namespace Minimart_Api.Services.RabbitMQ
{
    public class OrderEvent
    {
        public string OrderID { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty; // Identity User ID
        public List<ProductDto> products { get; set; } = new();
        public string UserEmail { get; set; } = string.Empty;
        public string MerchantEmail { get; set; } = string.Empty;
        public string UserPhoneNumber { get; set; } = string.Empty;
        public string MerchantPhoneNumber { get; set; } = string.Empty;
        public string addresses { get; set; } = string.Empty;
        public double Amount { get; set; }
    }
}