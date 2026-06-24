using Minimart_Api.Models.Enums;

namespace Minimart_Api.DTOS.Orders
{
    public class OrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty; // Identity User ID
        public string Status { get; set; } = string.Empty;
        public OrderStatusEnum StatusEnum { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryScheduleDate { get; set; }
        public string OrderedBy { get; set; } = string.Empty;
        public string PaymentConfirmation { get; set; } = string.Empty;
        
        // Financial Information
        public decimal SubTotal { get; set; }
        public decimal TotalDeliveryFees { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Address and Payment
        public ShippingAddressResponse? ShippingAddress { get; set; }
        public PaymentResponse? PaymentDetails { get; set; }
        
        // Items grouped by merchant
        public List<MerchantOrderGroup> MerchantGroups { get; set; } = new();
        
        // Tracking Information
        public List<OrderTrackingResponse> TrackingHistory { get; set; } = new();
        
        // Additional Properties
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class MerchantOrderGroup
    {
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantEmail { get; set; } = string.Empty;
        public string MerchantPhone { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class OrderItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Discount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ShippingAddressResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? DeliveryInstructions { get; set; }
    }

    public class PaymentResponse
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class OrderTrackingResponse
    {
        public string TrackingId { get; set; } = string.Empty;
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime TrackingDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Carrier { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}