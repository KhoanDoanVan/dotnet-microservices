
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using OrderService.Data;
using OrderService.Models;
using OrderService.DTOs;

namespace OrderService.Services;


public interface IOrderService
{
    Task<List<OrderDto>> GetAllOrdersAsync(int? userId, bool isAdmin);
    Task<OrderDto?> GetOrderByIdAsync(int id, int userId, bool isAdmin);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int userId, string token);
}


public class OrderService: IOrderService
{
    private readonly OrderDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrderService(OrderDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }


    public async Task<List<OrderDto>> GetAllOrdersAsync(int? userId, bool isAdmin)
    {
        var query = _context.Orders.Include(o => o.OrderItems).AsQueryable();

        if (!isAdmin && userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var orders = await query.ToListAsync();
        return orders.Select(MapToDto).ToList();
    }


    public async Task<OrderDto?> GetOrderByIdAsync(int id, int userId, bool isAdmin)
    {
        var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null)
        {
            return null;
        }

        if (!isAdmin && order.UserId != userId)
        {
            return null;
        }

        return MapToDto(order);
    }


    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, int userId, string token)
    {
        var order = new Order
        {
            CustomerId = request.CustomerId,
            UserId = userId,
            PromoId = request.PromoId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            DiscountAmount = request.DiscountAmount,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await GetProductFromServiceAsync(item.ProductId, token);

            if (product == null)
            {
                throw new Exception($"Product with ID {item.ProductId} not found");
            }

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * item.Quantity
            };

            order.OrderItems.Add(orderItem);
            totalAmount += orderItem.TotalPrice;
        }

        order.TotalAmount = totalAmount - order.DiscountAmount;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return MapToDto(order);
    }


    private async Task<ProductDto?> GetProductFromServiceAsync(int productId, string token)
    {
        // Create a Client with idenity is "ProductService"
        var client = _httpClientFactory.CreateClient("ProductService"); // Define Configuration at Program.cs
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/products/{productId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }


    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            UserId = order.UserId,
            PromoId = order.PromoId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString().ToLower(),
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                OrderItemId = oi.OrderItemId,
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}