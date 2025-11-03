namespace AuthService.Models;


public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Staff;
    public Datetime CreatedAt { get; set; } = DateTime.UtcNow;

}



public enum UserRole
{
    Admin,
    Staff,
    User
}


public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public Datetime CreatedAt { get; set; } = DateTime.UtcNow;
}


public class RefreshToken
{
    public int TokenId { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public Datetime ExpiresAt { get; set; }
    public Datetime CreatedAt { get; set; } = DateTime.UtcNow;
}