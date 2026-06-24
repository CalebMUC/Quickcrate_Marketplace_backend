using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Order
{
    public interface IorderRepository
    {
        // Enhanced CRUD Operations
        Task<ServiceResult<bool>> CreateOrderAsync(Models.Order order);
        Task<Models.Order?> GetOrderByIdAsync(string orderId);
        // Updated to use string userId (Identity system)
        Task<List<Models.Order>> GetUserOrdersAsync(string userId, int? statusId = null);
        Task<PagedOrderResponse> GetOrdersAsync(OrderFilterRequest filter);
        Task<bool> UpdateOrderAsync(Models.Order order);
        Task<bool> DeleteOrderAsync(string orderId);

        // Order Products
        Task<bool> CreateOrderProductAsync(OrderProduct orderProduct);
        Task<List<OrderProduct>> GetOrderProductsAsync(string orderId);

        // Status Operations
        Task<bool> UpdateOrderStatusAsync(string orderId, int newStatusId, string updatedBy);
        Task<bool> CancelOrderAsync(string orderId, string reason, string cancelledBy);

        // Tracking Operations
        Task<List<OrderTracking>> GetOrderTrackingByOrderIdAsync(string orderId);
        Task<bool> AddTrackingUpdateAsync(OrderTracking tracking);

        // Summary and Analytics - Updated to use string userId
        Task<OrderSummaryResponse> GetOrderSummaryAsync(string userId = null, Guid? merchantId = null);
        Task<List<OrderResponse>> GetRecentOrdersAsync(int count = 10, string userId = null);

        // Product and Merchant Lookups
        Task<Product?> GetProductByIdAsync(Guid productId);
        Task<Merchants?> GetMerchantByIdAsync(Guid merchantId);

        // Legacy Methods (maintained for backward compatibility) - Updated to use string userId
        Task<List<GetOrdersDTO>> GetOrdersByStatusAsync(int status, string userID);

        public Task<List<GetOrdersDTO>> GetUserOrdersAsync(string userId);
        Task<List<OrderStatus>> GetOrderStatusAsync();
        Task<Status> UpdateOrderStatusAsync(OrderTrackingDTO orderTracking);
        //Task<List<OrderTracking>> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus);
        Task<List<GetOrderTracking>> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus);
        Task<List<GetOrdersDTO>> GetOrdersByIdAsync(string OrderId);
        Task<List<MerchantOrderDto>> GetMerchantOrdersAsync(MerchantRequestDto requestDto);
        Task<List<MerchantOrderDto>> GetAdminOrdersAsync();
        Task<Status> AddOrder(OrderListDto transaction);

        /// <summary>
        /// Get detailed order progress showing status of each product
        /// </summary>
        //Task<OrderProgressResponse> GetOrderProgressAsync(string orderId);

        /// <summary>
        /// Checks if an order can be marked as completed
        /// </summary>
        //Task<bool> CanCompleteOrderAsync(string orderId);

        /// <summary>
        /// Gets summary of orders by status for reporting
        /// </summary>
        //Task<Dictionary<string, int>> GetOrderStatusSummaryAsync(Guid? merchantId = null);
    }

    // Service result for repository operations
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ServiceResult<T> Success(T data, string message = "Operation successful")
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static ServiceResult<T> Failure(string message, List<string> errors = null)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
