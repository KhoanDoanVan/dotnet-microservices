
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthService.Data;
using AuthService.Models;


namespace AuthService.Services;


public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task SaveRefreshTokenAsync(int userId, string refreshToken);
    // Task<T>: Thực hiện async và trả về giá trị
}



public class TokenService: ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AuthDbContext _context;

    public TokenService(IConfiguration configuration, AuthDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!));


        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var expirationMinutes = int.Parse(
            jwtSettings["AccessTokenExpirationMinutes"] ?? "60"
        );

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"], // Ai là người phát hành token
            audience: jwtSettings["Audience"], // Token dành cho ai (client nào)
            claims: claims, // Danh sách thông tin (payload) lưu trong token
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes), // Thời gian hết hạn
            signingCredentials: credentials // Cách ký token (private key)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }


    public async Task SaveRefreshTokenAsync(int userId, string refreshToken) // Thực hiện việc async, không trả giá trị
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");

        var token = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
    }
}