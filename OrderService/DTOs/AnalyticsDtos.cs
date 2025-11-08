namespace OrderService.DTOs;


public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TotalPaymentsReceived { get; set; }
    public decimal GrossSales { get; set; }
}


public class TopProductDto
{
    public int ProductId { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}


public class RevenueByDateDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}


public class PaymentMethodStatsDto
{
    public List<PaymentMethodBreakdown> Breakdown { get; set; } = new();
    public decimal TotalPayments { get; set; }
    public int TotalTransactions { get; set; }
}


public class PaymentMethodBreakdown
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}


public class PromotionEffectivenessDto
{
    public int TotalOrdersWithPromo { get; set; }
    public decimal TotalDiscountsGiven { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueWithoutDiscounts { get; set; }
    public decimal DiscountPercentage { get; set; }
    public List<PromotionBreakdown> Breakdown { get; set; } = new();
}



public class PromotionBreakdown
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalRevenue { get; set; }
}