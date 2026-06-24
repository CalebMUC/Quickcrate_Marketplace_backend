using Minimart_Api.DTOS.Orders;

namespace Minimart_Api.Services.NotificationService
{
    public interface INotfication
    {
        public  Task NotifyCustomer(OrderEvent orderEvent);
        public  Task NotifyMerchants(OrderEvent orderEvent);
    }
}
