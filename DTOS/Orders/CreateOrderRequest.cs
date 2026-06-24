using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty; // Identity User ID

        [Required]
        public List<OrderItemRequest> Items { get; set; } = new();

        [Required]
        public ShippingAddressRequest ShippingAddress { get; set; }

        public PickupLocationRequest? PickupLocation { get; set; }

        public PaymentRequest PaymentDetails { get; set; }

        public string? Notes { get; set; }

        public DateTime? PreferredDeliveryDate { get; set; }
    }

    public class OrderItemRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        public decimal? SpecialPrice { get; set; }
    }

    public class ShippingAddressRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string AddressLine1 { get; set; } = string.Empty;

        public string? AddressLine2 { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string County { get; set; } = string.Empty;

        public string? PostalCode { get; set; }

        public string? DeliveryInstructions { get; set; }
    }

    public class PickupLocationRequest
    {
        [Required]
        public int CountyId { get; set; }

        [Required]
        public int TownId { get; set; }

        [Required]
        public int DeliveryStationId { get; set; }
    }

    public class PaymentRequest
    {
        [Required]
        public string PaymentMethod { get; set; } = string.Empty; // MPESA, CARD, etc.

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? PaymentReference { get; set; }
    }
}