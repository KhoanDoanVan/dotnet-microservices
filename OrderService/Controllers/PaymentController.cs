using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;


[ApiController]
[Route("api/orders/{orderId}/payments")]
[Authorize]
public class PaymentsController: ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaymentsByOrderId(int orderId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId, userId, isAdmin);

        return Ok(payments);
    }


    [HttpPost]
    public async Task<IActionResult> CreatePayment(int orderId, [FromBody] CreatePaymentRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var payment = await _paymentService.CreatePaymentAsync(orderId, request, userId, isAdmin);

        if (payment == null)
        {
            return NotFound(new { message = "Order not found or access denied" });
        }

        return CreatedAtAction(nameof(GetPaymentsByOrderId), new { orderId = orderId }, payment);
    }
}