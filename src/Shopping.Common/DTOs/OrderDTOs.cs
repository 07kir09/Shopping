using System;

namespace Shopping.Common.DTOs;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
}

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 