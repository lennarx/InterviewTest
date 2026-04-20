using Azure.Messaging.ServiceBus;

namespace DotNetExamApi.Infrastructure;

public class ServiceBusPublisher : IDisposable
{
    private const string ConnectionString =
        "Endpoint=sb://dotnetexamapi.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=sK3r9mP2xNqL5vT8yW1jC4hB6dF0aE7uZ=";

    private const string QueueName = "order-events";

    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private bool _disposed;

    public ServiceBusPublisher()
    {
        _client = new ServiceBusClient(ConnectionString);
        _sender = _client.CreateSender(QueueName);
    }

    public async Task PublishOrderCreatedAsync(int orderId)
    {
        try
        {
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                OrderId = orderId,
                Event = "OrderCreated",
                Timestamp = DateTime.UtcNow
            });

            var message = new ServiceBusMessage(body)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString()
            };

            await _sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish order event for order {orderId}: {ex.Message}");
        }
    }

    public async Task PublishOrderStatusChangedAsync(int orderId, string newStatus)
    {
        try
        {
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                OrderId = orderId,
                Event = "OrderStatusChanged",
                Status = newStatus,
                Timestamp = DateTime.UtcNow
            });

            await _sender.SendMessageAsync(new ServiceBusMessage(body));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to publish status change for order {orderId}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _sender?.DisposeAsync().AsTask().Wait();
            _client?.DisposeAsync().AsTask().Wait();
        }
        _disposed = true;
    }
}
