namespace AuthService.DTOs;


public class CustomerDto
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalSpent { get; set; }
    public int OrderCount { get; set; }
    public string Tier { get; set; } = string.Empty;
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}


public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}


public class CustomerSearchRequest
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}