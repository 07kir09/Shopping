using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shopping.Common.DTOs;
using Shopping.Common.Interfaces;
using Shopping.Common.Models;

namespace Shopping.OrdersService.Controllers;

/// <summary>
/// Контроллер для управления заказами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Создать новый заказ с автоматическим списанием средств
    /// </summary>
    /// <param name="request">Данные для создания заказа</param>
    /// <returns>Созданный заказ</returns>
    /// <response code="200">Заказ успешно создан и оплачен</response>
    /// <response code="400">Недостаточно средств на счету или некорректные данные</response>
    /// <response code="404">Аккаунт пользователя не найден</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Внутренняя ошибка сервера при создании заказа");
        }
    }

    /// <summary>
    /// Получить все заказы пользователя
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <returns>Список заказов пользователя</returns>
    /// <response code="200">Список заказов успешно получен</response>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
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