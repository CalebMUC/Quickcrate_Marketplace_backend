using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Orders
{
    public class OrderFilterRequest
    {
        public string? ApplicationUserId { get; set; } // Identity User ID
        public Guid? MerchantId { get; set; }
        public List<int>? StatusIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "OrderDate";
        public bool SortDescending { get; set; } = true;
    }

    public class UpdateOrderStatusRequest
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int NewStatusId { get; set; }

        public string? StatusMessage { get; set; }

        [Required]
        public string UpdatedBy { get; set; } = string.Empty;

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string? Carrier { get; set; }
    }

    public class OrderTrackingRequest
    {
        public string? OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public string? ApplicationUserId { get; set; } // Identity User ID
    }

    public class OrderSummaryResponse
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public List<OrderResponse> RecentOrders { get; set; } = new();
    }

    public class PagedOrderResponse
    {
        public List<OrderResponse> Orders { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}