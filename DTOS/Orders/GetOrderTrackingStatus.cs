namespace Minimart_Api.DTOS.Orders
{
    public class GetOrderTrackingStatus
    {
        public Guid ProductID { get; set; }
        public string OrderID { get; set; }
    }
}
