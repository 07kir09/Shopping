using System;

namespace Shopping.Common.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; }
    public string Payload { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
} 