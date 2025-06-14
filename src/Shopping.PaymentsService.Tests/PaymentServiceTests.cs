using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shopping.Common.Messaging;
using Shopping.PaymentsService.Data;
using Shopping.PaymentsService.Models;
using Shopping.PaymentsService.Services;
using Xunit;

namespace Shopping.PaymentsService.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<ILogger<PaymentService>> _loggerMock;
        private readonly DbContextOptions<PaymentsDbContext> _options;

        public PaymentServiceTests()
        {
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _loggerMock = new Mock<ILogger<PaymentService>>();
            _options = new DbContextOptionsBuilder<PaymentsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreatePayment_ValidPayment_ReturnsCreatedPayment()
        {
            // Arrange
            using var context = new PaymentsDbContext(_options);
            var service = new PaymentService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var payment = new Payment
            {
                OrderId = Guid.NewGuid(),
                Amount = 100,
                Status = PaymentStatus.Pending
            };

            // Act
            var result = await service.CreatePaymentAsync(payment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payment.OrderId, result.OrderId);
            Assert.Equal(payment.Amount, result.Amount);
            Assert.Equal(PaymentStatus.Pending, result.Status);
            Assert.NotNull(result.CreatedAt);
        }

        [Fact]
        public async Task GetPayment_ExistingPayment_ReturnsPayment()
        {
            // Arrange
            using var context = new PaymentsDbContext(_options);
            var service = new PaymentService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var payment = new Payment
            {
                OrderId = Guid.NewGuid(),
                Amount = 100,
                Status = PaymentStatus.Pending
            };
            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetPaymentAsync(payment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(payment.Id, result.Id);
            Assert.Equal(payment.OrderId, result.OrderId);
        }

        [Fact]
        public async Task GetPayment_NonExistingPayment_ReturnsNull()
        {
            // Arrange
            using var context = new PaymentsDbContext(_options);
            var service = new PaymentService(context, _messagePublisherMock.Object, _loggerMock.Object);

            // Act
            var result = await service.GetPaymentAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePaymentStatus_ValidStatus_UpdatesPayment()
        {
            // Arrange
            using var context = new PaymentsDbContext(_options);
            var service = new PaymentService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var payment = new Payment
            {
                OrderId = Guid.NewGuid(),
                Amount = 100,
                Status = PaymentStatus.Pending
            };
            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            // Act
            var result = await service.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.Completed);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PaymentStatus.Completed, result.Status);
        }

        [Fact]
        public async Task UpdatePaymentStatus_InvalidPayment_ThrowsException()
        {
            // Arrange
            using var context = new PaymentsDbContext(_options);
            var service = new PaymentService(context, _messagePublisherMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                service.UpdatePaymentStatusAsync(Guid.NewGuid(), PaymentStatus.Completed));
        }

        [Fact]
        public async Task ProcessPayment_ValidPayment_UpdatesStatusAndPublishesMessage()
        {
            // Arrange
            using var context = new PaymentsDbContext(_options);
            var service = new PaymentService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var payment = new Payment
            {
                OrderId = Guid.NewGuid(),
                Amount = 100,
                Status = PaymentStatus.Pending
            };
            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            // Act
            await service.ProcessPaymentAsync(payment.Id);

            // Assert
            var updatedPayment = await context.Payments.FindAsync(payment.Id);
            Assert.Equal(PaymentStatus.Completed, updatedPayment.Status);
            _messagePublisherMock.Verify(x => x.Publish(
                It.IsAny<string>(),
                It.Is<string>(s => s == "payment.completed")),
                Times.Once);
        }
    }
} 