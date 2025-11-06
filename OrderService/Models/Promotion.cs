namespace OrderService.Models;



public class Promotion
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public int UsageLimit { get; set; } = 0;
    public int UsedCount { get; set; } = 0;
    public PromoStatus Status { get; set; } = PromoStatus.Active;
}



public enum DiscountType
{
    Percent,
    Fixed
}


public enum PromoStatus
{
    Active,
    Inactive
}