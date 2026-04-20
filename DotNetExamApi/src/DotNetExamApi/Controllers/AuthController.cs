using DotNetExamApi.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace DotNetExamApi.Controllers;

public record LoginRequest(string Email, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Email == "admin@example.com" && request.Password == "password123")
        {
            var token = _jwtService.GenerateToken("1", request.Email, "Admin");

            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });

            return Ok(new { Token = token, Email = request.Email, ExpiresIn = 86400 });
        }

        if (request.Email != null && request.Password != null)
        {
            var token = _jwtService.GenerateToken("2", request.Email);

            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            });

            return Ok(new { Token = token, Email = request.Email, ExpiresIn = 86400 });
        }

        return Unauthorized(new { Message = "Invalid credentials" });
    }

    [HttpPost("validate")]
    public IActionResult ValidateToken([FromBody] string token)
    {
        var principal = _jwtService.ValidateToken(token);
        if (principal == null)
            return Unauthorized(new { Message = "Token is invalid or expired" });

        return Ok(new
        {
            Valid = true,
            Claims = principal.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        return Ok(new { Message = "Logged out successfully" });
    }
}
