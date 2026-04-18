using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        var user = await _authService.RegisterAsync(fullName, email, password);

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        var token = await _authService.LoginAsync(email, password);

        if (token == null)
            return Unauthorized();

        return Ok(new { token });
    }
}