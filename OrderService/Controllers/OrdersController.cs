using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OrderService.DTOs;
using OrderService.Services;


namespace OrderService.Controllers;


[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
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


    // Extend
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";


        try
        {
            var order = await _orderService.CancelOrderAsync(id, userId, isAdmin);

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


    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(string status)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;


        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var orders = await _orderService.GetOrdersByStatusAsync(status, userId, isAdmin);

        return Ok(orders);
    }


    [HttpGet("date-range")]
    public async Task<IActionResult> GetOrdersByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate
    )
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;


        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate, userId, isAdmin);

        return Ok(orders);
    }


    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentOrders(
        [FromQuery] int count = 10
    )
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var orders = await _orderService.GetRecentOrdersAsync(count, userId, isAdmin);


        return Ok(orders);
    }



    [HttpGet("summary")]
    public async Task<IActionResult> GetOrderSummary()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var isAdmin = role?.ToLower() == "admin";
        var summary = await _orderService.GetOrderSummaryAsync(userId, isAdmin);

        return Ok(summary);
    }

}