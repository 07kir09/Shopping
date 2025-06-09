using System;
using System.Threading.Tasks;
using Shopping.Common.Models;

namespace Shopping.Common.Interfaces;

public interface IPaymentService
{
    Task<Account> GetAccountAsync(Guid userId);
    Task<Account> CreateAccountAsync(Guid userId);
    Task<bool> ProcessPaymentAsync(Guid userId, decimal amount, Guid orderId);
    Task<bool> ProcessRefundAsync(Guid userId, decimal amount, Guid orderId);
    Task ProcessOutboxMessagesAsync();
} 