namespace ProductService.Models;


public class Inventory
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}