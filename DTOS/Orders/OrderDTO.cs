using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Orders
{
    public class OrderDTO
    {
        public string? OrderID { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty; // Identity User ID
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryScheduleDate { get; set; }
        public string? OrderedBy { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<PaymentDetailsDto> PaymentDetails { get; set; } = new();
        public List<OrderProductsDTO> Products { get; set; } = new();
        public string? PaymentConfirmation { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal TotalDeliveryFees { get; set; }
        public decimal TotalTax { get; set; }
        public ShippingAddress? ShippingAddress { get; set; }
        public PickUpLocation? PickUpLocation { get; set; }
    }

    public class OrderListDto
    {
        [Required]
        public IList<OrderDTO> Orders { get; set; } = new List<OrderDTO>();
    }
}
