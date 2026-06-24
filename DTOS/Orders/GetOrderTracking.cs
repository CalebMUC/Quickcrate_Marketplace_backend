using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.DTOS.Orders
{
    public class GetOrderTracking
    {
        public string TrackingID { get; set; } = string.Empty;

        public Guid MerchantID { get; set; }

        public DateTime TrackingDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpectedDeliveryDate { get; set; }

        public string? PreviousStatus { get; set; }

        public string CurrentStatus { get; set; } = "Processing";

        public string? Carrier { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
    }
}
