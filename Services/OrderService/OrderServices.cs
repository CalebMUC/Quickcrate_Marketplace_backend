using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.Repositories.Order;
using Minimart_Api.Services.OrderService.OrderService;
using Minimart_Api.Models;
using Minimart_Api.DTOS.General;

namespace Minimart_Api.Services.OrderService
{
    public class OrderServices : IOrderService
    {
        private readonly IorderRepository _orderRepository;

        public OrderServices(IorderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<GetOrdersDTO>> GetOrdersByStatusAsync(int status, string userID)
        {
            return await _orderRepository.GetOrdersByStatusAsync(status, userID);
        }

        public async Task<List<GetOrdersDTO>> GetUserOrdersAsync(string userID)
        {
            return await _orderRepository.GetUserOrdersAsync(userID);
        }

        public async Task<List<OrderStatus>> GetOrderStatusAsync()
        {
            return await _orderRepository.GetOrderStatusAsync();
        }

        public async Task<Status> UpdateOrderStatusAsync(OrderTrackingDTO orderTracking)
        {
            return await _orderRepository.UpdateOrderStatusAsync(orderTracking);
        }
        public async Task<List<GetOrderTracking>> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus)
        {

            return await _orderRepository.GetOrderTrackingAsync(trackingStatus);
        }
        public async Task<List<GetOrdersDTO>> GetOrdersByIdAsync(string OrderId)
        {
            return await _orderRepository.GetOrdersByIdAsync(OrderId);
        }

        public async Task<List<MerchantOrderDto>> GetAdminOrdersAsync()
        {
            return await _orderRepository.GetAdminOrdersAsync();
        }

        public async Task<List<MerchantOrderDto>> GetMerchantOrdersAsync(MerchantRequestDto requestDto)
        {
            return await _orderRepository.GetMerchantOrdersAsync(requestDto);
        }

        public async Task<Status> AddOrder(OrderListDto orderDTO)
        {
            return await _orderRepository.AddOrder(orderDTO);
        }
    }
}
