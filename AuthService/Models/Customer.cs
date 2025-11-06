namespace AuthService.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalSpent { get; set; } = 0;
    public int OrderCount { get; set; } = 0;
    public CustomerTier Tier { get; set; } = CustomerTier.Bronze;
}


public enum CustomerTier
{
    Bronze,
    Silver,
    Gold,
    Platinum
}