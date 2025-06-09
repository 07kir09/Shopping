using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shopping.Common.DTOs;
using Shopping.Common.Interfaces;
using Shopping.Common.Models;

namespace Shopping.OrdersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var order = await _orderService.CreateOrderAsync(request);
        return Ok(new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Amount = order.Amount,
            Description = order.Description,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        });
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetUserOrders(Guid userId)
    {
        var orders = await _orderService.GetUserOrdersAsync(userId);
        var response = orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            Amount = o.Amount,
            Description = o.Description,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        });
        return Ok(response);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Amount = order.Amount,
            Description = order.Description,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        });
    }

    [HttpPut("{orderId}/status")]
    public async Task<ActionResult> UpdateOrderStatus(Guid orderId, [FromBody] string status)
    {
        var success = await _orderService.UpdateOrderStatusAsync(orderId, status);
        if (!success)
        {
            return NotFound();
        }

        return Ok();
    }
} 