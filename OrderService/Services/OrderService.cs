
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


    // Extend
    Task<List<OrderDto>> GetOrdersByStatusAsync(string status, int? userId, bool isAdmin);
    Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int? userId, bool isAdmin);
    Task<OrderDto?> CancelOrderAsync(int orderId, int userId, bool isAdmin);
    Task<OrderSummaryDto> GetOrderSummaryAsync(int? userId, bool isAdmin);
    Task<List<OrderDto>> GetRecentOrdersAsync(int count, int? userId, bool isAdmin);
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

    // Extend
    public async Task<List<OrderDto>> GetOrdersByStatusAsync(string status, int? userId, bool isAdmin)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .AsQueryable();

        if (!isAdmin && userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
        {
            query = query.Where(o => o.Status == orderStatus);
        }

        var orders = await query.ToListAsync();
        return orders.Select(MapToDto).ToList();
    }


    public async Task<List<OrderDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int? userId, bool isAdmin)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .AsQueryable();

        if (!isAdmin && userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var orders = await query.ToListAsync();
        return orders.Select(MapToDto).ToList();
    }


    public async Task<OrderDto?> CancelOrderAsync(int orderId, int userId, bool isAdmin)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            return null;
        }

        if (!isAdmin && order.UserId != userId)
        {
            return null;
        }

        if (order.Status == OrderStatus.Paid)
        {
            throw new Exception("Cannot cancel a paid order");
        }

        order.Status = OrderStatus.Canceled;
        await _context.SaveChangesAsync();

        return MapToDto(order);
    }


    public async Task<OrderSummaryDto> GetOrderSummaryAsync(int? userId, bool isAdmin)
    {
        var query = _context.Orders.AsQueryable();

        if (!isAdmin && userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var totalOrders = await query.CountAsync();
        var pendingOrders = await query.Where(o => o.Status == OrderStatus.Pending).CountAsync();
        var paidOrders = await query.Where(o => o.Status == OrderStatus.Paid).CountAsync();
        var canceledOrders = await query.Where(o => o.Status == OrderStatus.Canceled).CountAsync();
        var totalRevenue = await query.Where(o => o.Status == OrderStatus.Paid).SumAsync(o => o.TotalAmount);
        var totalDiscounts = await query.SumAsync(o => o.DiscountAmount);

        return new OrderSummaryDto
        {
            TotalOrders = totalOrders,
            PendingOrders = pendingOrders,
            PaidOrders = paidOrders,
            CanceledOrders = canceledOrders,
            TotalRevenue = totalRevenue,
            TotalDiscounts = totalDiscounts,
            AverageOrderValue = totalOrders > 0 ? totalRevenue / paidOrders : 0
        };
    }


    public async Task<List<OrderDto>> GetRecentOrdersAsync(int count, int? userId, bool isAdmin)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.OrderDate)
            .AsQueryable();

        if (!isAdmin && userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var orders = await query.Take(count).ToListAsync();
        return orders.Select(MapToDto).ToList();
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
            }).ToList(),
            Payments = order.Payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                OrderId = p.OrderId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString().ToLower(),
                PaymentDate = p.PaymentDate
            }).ToList()
        };
    }
}