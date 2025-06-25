using Shopping.Common.DTOs;

namespace Shopping.OrdersService.Services;
 
public interface IPaymentClient
{
    Task<AccountResponse?> GetAccountAsync(Guid userId);
    Task<bool> ProcessPaymentAsync(Guid userId, decimal amount, Guid orderId);
} 