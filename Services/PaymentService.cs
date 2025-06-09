using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shopping.Common.Interfaces;
using Shopping.Common.Messaging;
using Shopping.Common.Models;
using Shopping.PaymentsService.Data;

namespace Shopping.PaymentsService.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentsDbContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        PaymentsDbContext context,
        IMessagePublisher messagePublisher,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<Account> GetAccountAsync(Guid userId)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<Account> CreateAccountAsync(Guid userId)
    {
        var account = new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new account for user {UserId}", userId);
        return account;
    }

    public async Task<bool> ProcessPaymentAsync(Guid userId, decimal amount, Guid orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
            {
                _logger.LogWarning("Account not found for user {UserId}", userId);
                return false;
            }

            // Проверяем, не был ли уже обработан этот заказ
            var existingPayment = await _context.OutboxMessages
                .AnyAsync(m => m.MessageType == "PaymentProcessed" && 
                             m.Payload.Contains(orderId.ToString()));

            if (existingPayment)
            {
                _logger.LogInformation("Payment for order {OrderId} was already processed", orderId);
                await transaction.CommitAsync();
                return true;
            }

            if (account.Balance < amount)
            {
                _logger.LogWarning("Insufficient funds for user {UserId}", userId);
                return false;
            }

            // Атомарное обновление баланса с проверкой
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Accounts SET Balance = Balance - {0}, UpdatedAt = {1} " +
                "WHERE Id = {2} AND Balance >= {0}",
                amount, DateTime.UtcNow, account.Id);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Failed to update balance for user {UserId}", userId);
                await transaction.RollbackAsync();
                return false;
            }

            // Создаем сообщение о списании
            var paymentMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "PaymentProcessed",
                Payload = JsonSerializer.Serialize(new
                {
                    OrderId = orderId,
                    UserId = userId,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow
                }),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(paymentMessage);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Successfully processed payment for order {OrderId}", orderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> ProcessRefundAsync(Guid userId, decimal amount, Guid orderId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
            {
                _logger.LogWarning("Account not found for user {UserId}", userId);
                return false;
            }

            // Проверяем, не был ли уже обработан этот возврат
            var existingRefund = await _context.OutboxMessages
                .AnyAsync(m => m.MessageType == "RefundProcessed" && 
                             m.Payload.Contains(orderId.ToString()));

            if (existingRefund)
            {
                _logger.LogInformation("Refund for order {OrderId} was already processed", orderId);
                await transaction.CommitAsync();
                return true;
            }

            // Атомарное обновление баланса
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Accounts SET Balance = Balance + {0}, UpdatedAt = {1} " +
                "WHERE Id = {2}",
                amount, DateTime.UtcNow, account.Id);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("Failed to update balance for user {UserId}", userId);
                await transaction.RollbackAsync();
                return false;
            }

            // Создаем сообщение о возврате
            var refundMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "RefundProcessed",
                Payload = JsonSerializer.Serialize(new
                {
                    OrderId = orderId,
                    UserId = userId,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow
                }),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(refundMessage);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Successfully processed refund for order {OrderId}", orderId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for order {OrderId}", orderId);
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