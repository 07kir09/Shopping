using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Shopping.Common.Messaging;

public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "shopping";

    public RabbitMQPublisher(string hostName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true);
    }

    public Task PublishAsync(string messageType, string payload)
    {
        var body = Encoding.UTF8.GetBytes(payload);
        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: messageType,
            basicProperties: null,
            body: body);
            
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 