namespace Minimart_Api.DTOS.Orders
{
    public class OrderRequest
    {
        public int Status { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty; // Identity User ID
    }
}
