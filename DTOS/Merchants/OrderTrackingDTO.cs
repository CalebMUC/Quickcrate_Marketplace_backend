namespace Minimart_Api.DTOS.Merchants
{
    public class OrderTrackingDTO
    {
        public string TrackingID { get; set; }
        public int StatusId { get; set; }

        //public string Description { get; set; }
        public string OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string UpdatedBy { get; set; }

    }
}
