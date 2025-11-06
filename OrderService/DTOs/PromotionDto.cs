namespace OrderService.DTOs;


public class PromotionDto
{
    public int PromoId { get; set; }
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public string Status { get; set; } = string.Empty;
}


public class CreatePromotionRequest
{
    public string PromoCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percent";
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; } = 0;
    public int UsageLimit { get; set; } = 0;
}


public class UpdatePromotionRequest
{
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percent";
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int UsageLimit { get; set; }
    public string Status { get; set; } = "active";
}


public class ValidatePromotionRequest
{
    public string PromoCode { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}


public class PromotionValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public PromotionDto? Promotion { get; set; }
}