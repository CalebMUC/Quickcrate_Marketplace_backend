using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Payments;

namespace Minimart_Api.DTOS.Orders
{
    public class GetOrdersDTO
    {
        public string OrderID { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalOrderAmount { get; set; }
        public DateTime DeliveryScheduleDate { get; set; }
        public string? OrderedBy { get; set; }
        public List<PaymentDetailsDto> PaymentDetails { get; set; }
        public List<OrderProductsDTO> Products { get; set; }
        public string PaymentConfirmation { get; set; }
        public double TotalPaymentAmount { get; set; }
        public double TotalDeliveryFees { get; set; }
        public double TotalTax { get; set; }
        public ShippingAddress ShippingAddress { get; set; }
        public PickUpLocation PickUpLocation { get; set; }
        public string ImageUrl { get; set; }
    }
}
