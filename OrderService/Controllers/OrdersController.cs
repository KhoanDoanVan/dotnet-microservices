using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OrderService.DTOs;
using OrderService.Services;


namespace OrderService.Controllers;


[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController: ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders() // IActionResult: Standard Return Controller
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var orders = await _orderService.GetAllOrdersAsync(userId, isAdmin);

        return Ok(orders);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var order = await _orderService.GetOrderByIdAsync(id, userId, isAdmin);

        if (order == null)
        {
            return NotFound(
                new
                {
                    message = "Order not found or access denied"
                }
            );
        }

        return Ok(order);
    }


    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request
    )
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        try
        {
            var order = await _orderService.CreateOrderAsync(request, userId, token);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
        }
        catch (Exception ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    }
}