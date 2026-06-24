using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.Models;

namespace Minimart_Api.Services.OrderService.OrderService
{
    public interface IOrderService
    {
        Task<List<GetOrdersDTO>> GetOrdersByStatusAsync(int status, string userID);
        public  Task<List<GetOrdersDTO>> GetUserOrdersAsync(string userId);
        Task<List<OrderStatus>> GetOrderStatusAsync();
        Task<Status> UpdateOrderStatusAsync(OrderTrackingDTO orderTracking);
        //Task<List<OrderTracking>> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus);
        Task<List<GetOrderTracking>> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus);
        Task<List<GetOrdersDTO>> GetOrdersByIdAsync(string OrderId);
        Task<List<MerchantOrderDto>> GetMerchantOrdersAsync(MerchantRequestDto requestDto); // This is the correct interface method declaration
        Task<List<MerchantOrderDto>> GetAdminOrdersAsync();
        public Task<Status> AddOrder(OrderListDto transaction);



    }
}
