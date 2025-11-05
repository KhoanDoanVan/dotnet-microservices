using Microsoft.EntityFrameworkCore;
using AuthService.Data;
using AuthService.Models;
using AuthService.DTOs;
using BCrypt.Net;



namespace AuthService.Services;


public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<List<UserDto>> GetAllUsersAsync();
}



public class AuthService: IAuthService
{

    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(
        AuthDbContext context,
        ITokenService tokenService
    ) {
        _context = context;
        _tokenService = tokenService;
    }


    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username)) {
            return null;
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Password = hashedPassword,
            FullName = request.FullName,
            Role = Enum.TryParse<UserRole>(request.Role, true, out var role) ? role : UserRole.User,
            CreatedAt = DateTime.UtcNow
        };


        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshTokenAsync(user.UserId, refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }



    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return null;
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshTokenAsync(user.UserId, refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }


    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.UtcNow);

        if (storedToken == null)
        {
            return null;
        }

        var user = await _context.Users.FindAsync(storedToken.UserId);

        if (user == null)
        {
            return null;
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Remove(storedToken);
        await _tokenService.SaveRefreshTokenAsync(user.UserId, newRefreshToken);


        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = MapToUserDto(user)
        };
    }


    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }


    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(MapToUserDto).ToList();
    }

    public UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.ToString().ToLower(),
            CreatedAt = user.CreatedAt
        };
    }

}