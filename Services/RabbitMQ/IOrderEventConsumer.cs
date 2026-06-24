namespace Minimart_Api.Services.RabbitMQ
{
    public interface IOrderEventConsumer
    {
        public void StartConsuming();
    }
}
