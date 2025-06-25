using System.Text;
using System.Text.Json;
using Shopping.Common.DTOs;

namespace Shopping.OrdersService.Services;

public class ProcessPaymentRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public Guid OrderId { get; set; }
}

public class PaymentClient : IPaymentClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentClient> _logger;

    public PaymentClient(IHttpClientFactory httpClientFactory, ILogger<PaymentClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("PaymentsService");
        _logger = logger;
    }

    public async Task<AccountResponse?> GetAccountAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting account for user {UserId}", userId);
            var response = await _httpClient.GetAsync($"api/Payments/accounts/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AccountResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Account not found for user {UserId}", userId);
                return null;
            }
            
            _logger.LogWarning("Failed to get account for user {UserId}. Status: {StatusCode}", 
                userId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account for user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ProcessPaymentAsync(Guid userId, decimal amount, Guid orderId)
    {
        try
        {
            _logger.LogInformation("Processing payment for user {UserId}, amount {Amount}, order {OrderId}", 
                userId, amount, orderId);
                
            var request = new ProcessPaymentRequest
            {
                UserId = userId,
                Amount = amount,
                OrderId = orderId
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("api/Payments/process", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Payment processed successfully for order {OrderId}", orderId);
                return true;
            }
            
            _logger.LogWarning("Payment failed for order {OrderId}. Status: {StatusCode}", 
                orderId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
            return false;
        }
    }
} 