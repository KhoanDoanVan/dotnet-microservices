using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.DTOs;

namespace OrderService.Services;


public interface IPaymentService
{
    Task<PaymentDto?> CreatePaymentAsync(int orderId, CreatePaymentRequest request, int userId, bool isAdmin);
    Task<List<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId, int userId, bool isAdmin);
}


public class PaymentService: IPaymentService
{
    private readonly OrderDbContext _context;


    public PaymentService(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> CreatePaymentAsync(
        int orderId, CreatePaymentRequest request, int userId, bool isAdmin
    )
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
        {
            return null;
        }

        if (!isAdmin && order.UserId != userId)
        {
            return null;
        }

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            paymentMethod = PaymentMethod.Cash;
        }

        var payment = new Payment
        {
            OrderId = orderId,
            Amount = request.Amount,
            PaymentMethod = paymentMethod,
            PaymentDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);


        // Check if Order is fully paid
        var totalPaid = await _context.Payments
            .Where(p => p.OrderId == orderId)
            .SumAsync(p => p.Amount) + payment.Amount;


        if (totalPaid >= order.TotalAmount)
        {
            order.Status = OrderStatus.Paid;
        }

        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }


    public async Task<List<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId, int userId, bool isAdmin)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
        {
            return new List<PaymentDto>();
        }

        if (!isAdmin && order.UserId != userId)
        {
            return new List<PaymentDto>();
        }

        var payments = await _context.Payments
            .Where(p => p.OrderId == orderId)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }


    private PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString().ToLower(),
            PaymentDate = payment.PaymentDate
        };
    }
}