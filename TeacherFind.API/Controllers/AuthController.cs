using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Auth;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IVerificationRepository _verificationRepository;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IVerificationRepository verificationRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = await _authService.RegisterAsync(request);

        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            Role = user.Role.ToString(),
            user.IsPhoneVerified,
            user.IsEmailVerified
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result == null)
            return Unauthorized(new { message = "Email veya şifre hatalı" });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Geçersiz token" });

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "Kullanıcı bulunamadı" });

        return Ok(new MeResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.ProfileImageUrl
        });
    }

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyDto dto)
    {
        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code);

        if (code == null)
            return BadRequest(new { message = "Kod yanlış" });

        var user = await _userRepository.GetByIdAsync(dto.UserId);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        user.IsPhoneVerified = true;

        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Telefon doğrulandı" });
    }
}