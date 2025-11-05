namespace OrderService.DTOs;



public class OrderDto
{
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public int UserId { get; set; }
    public int? PromoId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new();
}


public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}


public class CreateOrderRequest
{
    public int? CustomerId { get; set; }
    public int? PromoId { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}


public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}