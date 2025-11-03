using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Services;


namespace AuthService.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request
    ) {
        var result = await _authService.RegisterAsync(request);

        if (result == null)
        {
            return BadRequest(
                new {
                    message = "Username already exists"
                }
            );
        }

        return Ok(result);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request
    ) {
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized(
                new {
                    message = "Invalid username or password"
                }
            );
        }

        return Ok(result);
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request
    ) {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
        {
            return Unauthorized(
                new {
                    message = "Invalid or expired refresh token"
                }
            );
        }

        return Ok(result);
    }


    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(
                new {
                    message = "User not found"
                }
            );
        }

        return Ok(user);
    }


    [Authorize(Roles = "admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }
}