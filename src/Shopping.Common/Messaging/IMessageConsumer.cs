using System;
using System.Threading.Tasks;

namespace Shopping.Common.Messaging;

public interface IMessageConsumer
{
    Task StartConsumingAsync(string queueName, Func<string, Task> messageHandler);
    void StopConsuming();
} 