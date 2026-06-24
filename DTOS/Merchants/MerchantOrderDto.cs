namespace Minimart_Api.DTOS.Merchants
{
    public class MerchantOrderDto
    {
        public string OrderId { get; set; }
        public double SubTotal { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public List<MerchantOrderProductDto> Products { get; set; }

    }

    public class MerchantOrderProductDto
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

    }

    public class OrderWithStatus
    {
        public Models.Order Order { get; set; }
        public string StatusName { get; set; }
    }

}
