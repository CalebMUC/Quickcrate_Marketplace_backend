using Minimart_Api.DTOS.Orders;

namespace Minimart_Api.Services.RabbitMQ
{
    public interface IOrderEventPublisher
    {
        public Task PublishOrderEvent(OrderEvent orderEvent);
    }
}
