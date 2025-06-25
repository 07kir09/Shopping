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
    private readonly IPaymentClient _paymentClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        OrdersDbContext context,
        IMessagePublisher messagePublisher,
        IPaymentClient paymentClient,
        ILogger<OrderService> logger)
    {
        _context = context;
        _messagePublisher = messagePublisher;
        _paymentClient = paymentClient;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order for user {UserId}, amount {Amount}", request.UserId, request.Amount);
        var account = await _paymentClient.GetAccountAsync(request.UserId);
        if (account == null)
        {
            _logger.LogWarning("Account not found for user {UserId}", request.UserId);
            throw new InvalidOperationException("Аккаунт пользователя не найден. Создайте аккаунт сначала.");
        }

        if (account.Balance < request.Amount)
        {
            _logger.LogWarning("Insufficient funds for user {UserId}. Balance: {Balance}, Required: {Amount}", 
                request.UserId, account.Balance, request.Amount);
            throw new InvalidOperationException($"Недостаточно средств на счету. Доступно: {account.Balance:C}, требуется: {request.Amount:C}");
        }

        var orderId = Guid.NewGuid();
        var paymentSuccess = await _paymentClient.ProcessPaymentAsync(request.UserId, request.Amount, orderId);
        
        if (!paymentSuccess)
        {
            _logger.LogWarning("Payment failed for order {OrderId}", orderId);
            throw new InvalidOperationException("Ошибка при списании средств. Возможно, недостаточно средств на счету.");
        }
        try
        {
            var order = new Order
            {
                Id = orderId,
                UserId = request.UserId,
                Amount = request.Amount,
                Description = request.Description,
                Status = "Paid",
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
                    Status = order.Status,
                    Timestamp = DateTime.UtcNow
                }),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(outboxMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created and paid order {OrderId} for user {UserId}. Amount: {Amount}", 
                order.Id, order.UserId, order.Amount);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving order {OrderId} after successful payment", orderId);
            throw new InvalidOperationException("Заказ оплачен, но возникла ошибка при сохранении. Обратитесь в службу поддержки.");
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