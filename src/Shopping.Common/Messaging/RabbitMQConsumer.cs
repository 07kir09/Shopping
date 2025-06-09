using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Shopping.Common.Messaging;

public class RabbitMQConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "shopping";

    public RabbitMQConsumer(string hostName, string queueName, Action<string, string> messageHandler)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true);
        _channel.QueueDeclare(queueName, true, false, false, null);
        _channel.QueueBind(queueName, ExchangeName, queueName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            
            messageHandler(routingKey, message);
        };

        _channel.BasicConsume(queue: queueName,
                            autoAck: true,
                            consumer: consumer);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
} 