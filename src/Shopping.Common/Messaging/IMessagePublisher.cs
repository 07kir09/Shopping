using System.Threading.Tasks;

namespace Shopping.Common.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string messageType, string payload);
} 