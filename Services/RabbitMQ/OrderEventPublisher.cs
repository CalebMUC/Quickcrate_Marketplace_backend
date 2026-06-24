using System.Text;
using System.Text.Json;
using Minimart_Api.DTOS.Orders;
using RabbitMQ.Client;

namespace Minimart_Api.Services.RabbitMQ
{
    public class OrderEventPublisher : IOrderEventPublisher
    {
        private readonly IRabbitMqConnection _connection;
        private ILogger<OrderEventPublisher> _logger;

        public OrderEventPublisher(IRabbitMqConnection connection, ILogger<OrderEventPublisher> logger) { 
            _connection = connection;
            _logger = logger;
        }

        public async Task PublishOrderEvent(OrderEvent orderEvent)
        {
            try
            {
                var connection = await _connection.GetConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: "orderEvent_queue",
                    durable: true,
                    autoDelete: false,
                    exclusive: false,
                    arguments: null
                    );
                var messageBody = JsonSerializer.Serialize(orderEvent);
                var body = Encoding.UTF8.GetBytes(messageBody);

                //publish channel
                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: "orderEvent_queue",
                    mandatory: false,
                     basicProperties: new BasicProperties
                     {
                         ContentType = "application/json",
                         //DeliveryMode = '2' // Persistent delivery
                     },
                     body: body

                    );

                _logger.LogInformation($"{orderEvent} was published on {DateTime.Now}");
            }
            catch (Exception ex) {

                _logger.LogError($"Error While Publishing{ex.Message}");
            }
        }
    }
}
