using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shopping.Common.DTOs;
using Shopping.Common.Models;

namespace Shopping.Common.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderRequest request);
    Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
    Task<Order> GetOrderByIdAsync(Guid orderId);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);
    Task ProcessOutboxMessagesAsync();
} 