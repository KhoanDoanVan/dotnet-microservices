using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.DTOs;

namespace OrderService.Services;


public interface IAnalyticsService
{
    Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<List<TopProductDto>> GetTopProductsAsync(int count, DateTime? startDate, DateTime? endDate);
    Task<List<RevenueByDateDto>> GetRevenueByDateAsync(DateTime startDate, DateTime endDate);
    Task<PaymentMethodStatsDto> GetPaymentMethodStatsAsync(DateTime? startDate, DateTime? endDate);
    Task<PromotionEffectivenessDto> GetPromotionEffectivenessAsync(int? promoId, DateTime? startDate, DateTime? endDate);
}



public class AnalyticsService : IAnalyticsService
{
    private readonly OrderDbContext _context;


    public AnalyticsService(OrderDbContext context)
    {
        _context = context;
    }


    public async Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Paid)
            .ToListAsync();

        var totalSales = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        var totalDiscounts = orders.Sum(o => o.DiscountAmount);
        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        var payments = await _context.Payments
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .ToListAsync();

        var totalPayments = payments.Sum(p => p.Amount);

        return new SalesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            TotalDiscounts = totalDiscounts,
            AverageOrderValue = averageOrderValue,
            TotalPaymentsReceived = totalPayments,
            GrossSales = totalSales + totalDiscounts
        };
    }


    public async Task<List<TopProductDto>> GetTopProductsAsync(int count, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.OrderItems
            .Join(_context.Orders,
                oi => oi.OrderId,
                o => o.OrderId,
                (oi, o) => new { OrderItem = oi, Order = o })
            .Where(x => x.Order.Status == OrderStatus.Paid);

        if (startDate.HasValue)
        {
            query = query.Where(x => x.Order.OrderDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.Order.OrderDate <= endDate.Value);
        }

        var topProducts = await query
            .GroupBy(x => x.OrderItem.ProductId)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                TotalQuantitySold = g.Sum(x => x.OrderItem.Quantity),
                TotalRevenue = g.Sum(x => x.OrderItem.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(count)
            .ToListAsync();

        return topProducts;
    }


    public async Task<List<RevenueByDateDto>> GetRevenueByDateAsync(DateTime startDate, DateTime endDate)
    {
        var revenueByDate = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == OrderStatus.Paid)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new RevenueByDateDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                AverageOrderValue = g.Average(o => o.TotalAmount)
            })
            .OrderBy(r => r.Date)
            .ToListAsync();

        return revenueByDate;
    }


    public async Task<PaymentMethodStatsDto> GetPaymentMethodStatsAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Payments.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= endDate.Value);
        }

        var paymentStats = await query
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodBreakdown
            {
                PaymentMethod = g.Key.ToString().ToLower(),
                TotalAmount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        return new PaymentMethodStatsDto
        {
            Breakdown = paymentStats,
            TotalPayments = paymentStats.Sum(p => p.TotalAmount),
            TotalTransactions = paymentStats.Sum(p => p.Count)
        };
    }



    public async Task<PromotionEffectivenessDto> GetPromotionEffectivenessAsync(int? promoId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Orders.AsQueryable();

        if (promoId.HasValue)
        {
            query = query.Where(o => o.PromoId == promoId.Value);
        }
        else
        {
            query = query.Where(o => o.PromoId != null);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= endDate.Value);
        }

        var ordersWithPromo = await query.ToListAsync();
        var totalOrders = ordersWithPromo.Count;
        var totalDiscounts = ordersWithPromo.Sum(o => o.DiscountAmount);
        var totalRevenue = ordersWithPromo.Where(o => o.Status == OrderStatus.Paid).Sum(o => o.TotalAmount);
        var revenueWithoutDiscount = totalRevenue + totalDiscounts;

        var promoBreakdown = await query
            .Where(o => o.PromoId.HasValue)
            .Join(_context.Promotions,
                o => o.PromoId.Value,
                p => p.PromoId,
                (o, p) => new { Order = o, Promotion = p })
            .GroupBy(x => new { x.Promotion.PromoId, x.Promotion.PromoCode })
            .Select(g => new PromotionBreakdown
            {
                PromoId = g.Key.PromoId,
                PromoCode = g.Key.PromoCode,
                UsageCount = g.Count(),
                TotalDiscount = g.Sum(x => x.Order.DiscountAmount),
                TotalRevenue = g.Where(x => x.Order.Status == OrderStatus.Paid).Sum(x => x.Order.TotalAmount)
            })
            .ToListAsync();

        return new PromotionEffectivenessDto
        {
            TotalOrdersWithPromo = totalOrders,
            TotalDiscountsGiven = totalDiscounts,
            TotalRevenue = totalRevenue,
            RevenueWithoutDiscounts = revenueWithoutDiscount,
            DiscountPercentage = revenueWithoutDiscount > 0 ? (totalDiscounts / revenueWithoutDiscount) * 100 : 0,
            Breakdown = promoBreakdown
        };
    }
}