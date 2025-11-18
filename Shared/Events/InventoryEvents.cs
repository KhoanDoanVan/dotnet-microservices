namespace Shared.Events;



public class InventoryUpdatedEvent: DomainEvent
{
    public InventoryUpdatedEvent()
    {
        EventType = nameof(InventoryUpdatedEvent);
    }

    public int ProductId { get; set; }
    public int OldQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int ChangeAmount { get; set; }
    public string UpdateType { get; set; } = string.Empty; // "set","increment","decrement"
    public DateTime UpdatedAt { get; set; }
    public string? Reason { get; set; }
}


public class LowStockAlertEvent: DomainEvent
{
    public LowStockAlertEvent()
    {
        EventType = nameof(LowStockAlertEvent);
    }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int Threshold { get; set; }
    public DateTime AlertDate { get; set; }
}


public class OutOfStockEvent: DomainEvent
{
    public OutOfStockEvent()
    {
        EventType = nameof(OutOfStockEvent);
    }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime OutOfStockDate { get; set; }
}