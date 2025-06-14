using System;
using Microsoft.Extensions.Logging;
using Moq;
using Shopping.Common.Messaging;
using Xunit;

namespace Shopping.Common.Tests.Messaging
{
    public class RabbitMQPublisherTests
    {
        private readonly Mock<ILogger<RabbitMQPublisher>> _loggerMock;

        public RabbitMQPublisherTests()
        {
            _loggerMock = new Mock<ILogger<RabbitMQPublisher>>();
        }

        [Fact]
        public void Constructor_ValidHostName_CreatesInstance()
        {
            // Arrange & Act
            var publisher = new RabbitMQPublisher("localhost", _loggerMock.Object);

            // Assert
            Assert.NotNull(publisher);
        }

        [Fact]
        public void Constructor_NullHostName_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new RabbitMQPublisher(null, _loggerMock.Object));
        }

        [Fact]
        public void Publish_ValidMessage_PublishesSuccessfully()
        {
            // Arrange
            var publisher = new RabbitMQPublisher("localhost", _loggerMock.Object);
            var message = "test message";
            var routingKey = "test.key";

            // Act & Assert
            var exception = Record.Exception(() => publisher.Publish(message, routingKey));
            Assert.Null(exception);
        }

        [Fact]
        public void Publish_NullMessage_ThrowsArgumentNullException()
        {
            // Arrange
            var publisher = new RabbitMQPublisher("localhost", _loggerMock.Object);
            string message = null;
            var routingKey = "test.key";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => publisher.Publish(message, routingKey));
        }

        [Fact]
        public void Publish_NullRoutingKey_ThrowsArgumentNullException()
        {
            // Arrange
            var publisher = new RabbitMQPublisher("localhost", _loggerMock.Object);
            var message = "test message";
            string routingKey = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => publisher.Publish(message, routingKey));
        }

        [Fact]
        public void Dispose_DisposesResources()
        {
            // Arrange
            var publisher = new RabbitMQPublisher("localhost", _loggerMock.Object);

            // Act
            publisher.Dispose();

            // Assert
            // No exception should be thrown when trying to use disposed publisher
            Assert.Throws<ObjectDisposedException>(() => 
                publisher.Publish("test", "test.key"));
        }
    }
} 