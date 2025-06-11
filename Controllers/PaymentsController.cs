using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shopping.Common.DTOs;
using Shopping.Common.Interfaces;
using Shopping.Common.Models;

namespace Shopping.PaymentsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("accounts/{userId}")]
        public async Task<ActionResult<AccountResponse>> GetAccount(Guid userId)
        {
            var account = await _paymentService.GetAccountAsync(userId);
            if (account == null)
            {
                return NotFound();
            }

            return Ok(new AccountResponse
            {
                Id = account.Id,
                UserId = account.UserId,
                Balance = account.Balance,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt
            });
        }

        [HttpPost("accounts")]
        public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var account = await _paymentService.CreateAccountAsync(request.UserId);
            return CreatedAtAction(
                nameof(GetAccount),
                new { userId = account.UserId },
                new AccountResponse
                {
                    Id = account.Id,
                    UserId = account.UserId,
                    Balance = account.Balance,
                    CreatedAt = account.CreatedAt,
                    UpdatedAt = account.UpdatedAt
                });
        }

        [HttpPost("process")]
        public async Task<ActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            var success = await _paymentService.ProcessPaymentAsync(
                request.UserId,
                request.Amount,
                request.OrderId);

            if (!success)
            {
                return BadRequest("Payment processing failed");
            }

            return Ok();
        }

        [HttpPost("refund")]
        public async Task<ActionResult> ProcessRefund([FromBody] ProcessRefundRequest request)
        {
            var success = await _paymentService.ProcessRefundAsync(
                request.UserId,
                request.Amount,
                request.OrderId);

            if (!success)
            {
                return BadRequest("Refund processing failed");
            }

            return Ok();
        }
    }

    public class ProcessPaymentRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public Guid OrderId { get; set; }
    }

    public class ProcessRefundRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public Guid OrderId { get; set; }
    }
} 