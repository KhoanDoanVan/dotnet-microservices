namespace ProductService.DTOs;


public class InventoryDto
{
    public int InventoryId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}


public class UpdateInventoryRequest
{
    public int Quantity { get; set; }
}


public class AdjustInventoryRequest
{
    public int AdjustmentQuantity { get; set; }
}