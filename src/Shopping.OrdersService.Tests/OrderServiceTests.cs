using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Shopping.Common.Messaging;
using Shopping.OrdersService.Data;
using Shopping.OrdersService.Models;
using Shopping.OrdersService.Services;
using Xunit;

namespace Shopping.OrdersService.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IMessagePublisher> _messagePublisherMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly DbContextOptions<OrdersDbContext> _options;

        public OrderServiceTests()
        {
            _messagePublisherMock = new Mock<IMessagePublisher>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _options = new DbContextOptionsBuilder<OrdersDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreateOrder_ValidOrder_ReturnsCreatedOrder()
        {
            // Arrange
            using var context = new OrdersDbContext(_options);
            var service = new OrderService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var order = new Order
            {
                UserId = "testUser",
                Amount = 100,
                Description = "Test Order"
            };

            // Act
            var result = await service.CreateOrderAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.UserId, result.UserId);
            Assert.Equal(order.Amount, result.Amount);
            Assert.Equal(order.Description, result.Description);
            Assert.Equal(OrderStatus.Pending, result.Status);
            Assert.NotNull(result.CreatedAt);
        }

        [Fact]
        public async Task GetOrder_ExistingOrder_ReturnsOrder()
        {
            // Arrange
            using var context = new OrdersDbContext(_options);
            var service = new OrderService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var order = new Order
            {
                UserId = "testUser",
                Amount = 100,
                Description = "Test Order"
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetOrderAsync(order.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.Id, result.Id);
            Assert.Equal(order.UserId, result.UserId);
        }

        [Fact]
        public async Task GetOrder_NonExistingOrder_ReturnsNull()
        {
            // Arrange
            using var context = new OrdersDbContext(_options);
            var service = new OrderService(context, _messagePublisherMock.Object, _loggerMock.Object);

            // Act
            var result = await service.GetOrderAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateOrderStatus_ValidStatus_UpdatesOrder()
        {
            // Arrange
            using var context = new OrdersDbContext(_options);
            var service = new OrderService(context, _messagePublisherMock.Object, _loggerMock.Object);
            var order = new Order
            {
                UserId = "testUser",
                Amount = 100,
                Description = "Test Order",
                Status = OrderStatus.Pending
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act
            var result = await service.UpdateOrderStatusAsync(order.Id, OrderStatus.Completed);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderStatus.Completed, result.Status);
        }

        [Fact]
        public async Task UpdateOrderStatus_InvalidOrder_ThrowsException()
        {
            // Arrange
            using var context = new OrdersDbContext(_options);
            var service = new OrderService(context, _messagePublisherMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                service.UpdateOrderStatusAsync(Guid.NewGuid(), OrderStatus.Completed));
        }
    }
} 