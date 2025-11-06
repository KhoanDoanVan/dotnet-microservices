namespace OrderService.Models;


public class Order
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public int UserId { get; set; }
    public int? PromoId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}


public enum OrderStatus
{
    Pending,
    Paid,
    Canceled
}


public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
