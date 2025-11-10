using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Shared.Services;



public interface IMessageBusService
{
    Task PublishAsync<T>(string exchange, string routingKey, T message);
    Task SubscribeAsync<T>(string queue, string exchange, string routingKey, Func<T, Task> handler);
}



public class RabbitMQMessageBusService: IMessageBusService
{

    private readonly IConnection _connection;
    private readonly IModel _channel;


    public RabbitMQMessageBusService(string connectionString)
    {
        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            Uri = new Uri(connectionString),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(60)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }



    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        // Declare Exchange: đảm bảo exchange tồn tại (nếu chưa thì tạo mới).
        _channel.ExchangeDeclare(
            exchange,
            ExchangeType.Topic, // cho phép publish theo routing key pattern
            durable: true
        );


        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();

        properties.Persistent = true; // lưu message trên disk để tránh mất khi broker restart
        properties.ContentType = "application/json";
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());


        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        await Task.CompletedTask;
    }


    public async Task SubscribeAsync<T>(string queue, string exchange, string routingKey, Func<T, Task> handler)
    {
        // Declare exchange and queue
        _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true); // đảm bảo exchange tồn tại.
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false); // đảm bảo queue tồn tại.
        _channel.QueueBind(queue, exchange, routingKey); // nối queue với exchange thông qua routing key pattern.


        // Set QoS
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false); // consumer chỉ nhận 10 message một lúc (giúp backpressure).


        var consumer = new EventingBasicConsumer(_channel); // tạo consumer lắng nghe message đến queue.


        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await handler(message);
                }

                _channel.BasicAck(ea.DeliveryTag, multiple: false); // xác nhận đã xử lý thành công.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");

                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false); // xác nhận đã xử lý thất bại.
            }
        };


        _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer); // bắt đầu nhận message từ một queue.

        await Task.CompletedTask;
    }


    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}