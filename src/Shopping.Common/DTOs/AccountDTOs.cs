using System;

namespace Shopping.Common.DTOs;

public class CreateAccountRequest
{
    public Guid UserId { get; set; }
}

public class TopUpAccountRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
}

public class AccountResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
} 