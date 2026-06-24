using RabbitMQ.Client;


namespace Minimart_Api.Services.RabbitMQ
{
    public interface IRabbitMqConnection : IAsyncDisposable
    {
        Task<IConnection> GetConnectionAsync();
        bool IsConnected { get; }
    }
}