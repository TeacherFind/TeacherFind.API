using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Auth;
using TeacherFind.Domain.Entities;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/auth")]
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

    // Task 1 — Register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role)
            || (role != UserRole.Student && role != UserRole.Tutor))
        {
            return BadRequest(new { message = "Geçersiz rol. Sadece Student veya Tutor kabul edilir." });
        }

        try
        {
            var user = await _authService.RegisterAsync(dto.FullName, dto.Email, dto.Password, role);
            return Ok(new { message = "Kayıt başarılı.", userId = user.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Task 1 — Login returns full user info + token
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(new { message = "E-posta veya şifre hatalı." });

        var token = await _authService.LoginAsync(dto.Email, dto.Password);
        if (token is null)
            return Unauthorized(new { message = "E-posta veya şifre hatalı." });

        return Ok(new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        });
    }

    // Task 2 — GET /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(claim, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        return Ok(new MeResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.ProfileImageUrl
        });
    }

    // Verify phone/email code
    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyDto dto)
    {
        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code);
        if (code is null)
            return BadRequest(new { message = "Kod hatalı veya süresi dolmuş." });

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user is null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        code.IsUsed = true;
        user.IsPhoneVerified = true;
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Doğrulama başarılı." });
    }
}