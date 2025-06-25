using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
namespace Shopping.Common.Messaging;

public class RabbitMQPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;

    public RabbitMQPublisher(string hostName, ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = "guest",
                Password = "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("shopping", ExchangeType.Topic, true);
            
            _logger.LogInformation("Successfully connected to RabbitMQ at {HostName}", hostName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {HostName}", hostName);
            throw;
        }
    }

    public void Publish(string message, string routingKey)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                exchange: "shopping",
                routingKey: routingKey,
                basicProperties: null,
                body: body);
            
            _logger.LogInformation("Message published successfully with routing key {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message with routing key {RoutingKey}", routingKey);
            throw;
        }
    }

    public async Task PublishAsync(string messageType, string payload)
    {
        await Task.Run(() => Publish(payload, messageType));
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 