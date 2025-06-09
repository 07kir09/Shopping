using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopping.Common.DTOs;
using Shopping.Common.Interfaces;
using Shopping.Common.Messaging;
using Shopping.Common.Models;
using Shopping.OrdersService.Data;

namespace Shopping.OrdersService.Services;

public class OrderService : IOrderService
{
    private readonly OrdersDbContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        OrdersDbContext context,
        IMessagePublisher messagePublisher,
        ILogger<OrderService> logger)
    {
        _context = context;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                Description = request.Description,
                Status = "Created",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "OrderCreated",
                Payload = JsonSerializer.Serialize(new
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Amount = order.Amount,
                    Timestamp = DateTime.UtcNow
                }),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(outboxMessage);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Created new order {OrderId} for user {UserId}", order.Id, order.UserId);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", request.UserId);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId)
    {
        return await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return false;
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "OrderStatusUpdated",
                Payload = JsonSerializer.Serialize(new
                {
                    OrderId = order.Id,
                    Status = status,
                    Timestamp = DateTime.UtcNow
                }),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(outboxMessage);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Updated status of order {OrderId} to {Status}", orderId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status of order {OrderId}", orderId);
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task ProcessOutboxMessagesAsync()
    {
        var messages = await _context.OutboxMessages
            .Where(m => m.Status == "Pending")
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        foreach (var message in messages)
        {
            try
            {
                await _messagePublisher.PublishAsync(message.MessageType, message.Payload);
                message.Status = "Processed";
                message.ProcessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
            }
        }
    }
} 