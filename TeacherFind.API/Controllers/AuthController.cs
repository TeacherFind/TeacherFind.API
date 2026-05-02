using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Identity;
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
    private readonly IPasswordHasher _passwordHasher;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IVerificationRepository verificationRepository,
        IPasswordHasher passwordHasher)
    {
        _authService = authService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(request);
            var loginResult = await _authService.LoginAsync(request.Email, request.Password);
            return Ok(loginResult);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result == null)
            return Unauthorized(new { message = "Email veya şifre hatalı." });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { message = "Geçersiz token." });

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Unauthorized(new { message = "Kullanıcı bulunamadı." });

        return Ok(new MeResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.ProfileImageUrl,
            PhoneNumber = user.PhoneNumber,
            CityId = user.CityId,
            CityName = user.City?.Name
        });
    }

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyDto dto)
    {
        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code);
        if (code == null)
            return BadRequest(new { message = "Kod hatalı veya süresi dolmuş." });

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        code.IsUsed = true;
        user.IsPhoneVerified = true;
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Telefon doğrulandı." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null)
            return Ok(new { message = "Eğer bu e-posta kayıtlıysa sıfırlama kodu gönderildi." });

        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = new Random().Next(100000, 999999).ToString(),
            Type = "PasswordReset",
            ExpireAt = DateTime.UtcNow.AddMinutes(15)
        };

        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        Console.WriteLine($"[PASSWORD RESET CODE] {user.Email} -> {code.Code}");

        return Ok(new { message = "Sıfırlama kodu e-posta adresinize gönderildi." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Geçersiz istek." });

        var code = await _verificationRepository.GetValidCode(user.Id, dto.Code);
        if (code == null)
            return BadRequest(new { message = "Kod hatalı veya süresi dolmuş." });

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Şifre en az 6 karakter olmalıdır." });

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        code.IsUsed = true;
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Şifreniz başarıyla değiştirildi." });
    }

    // POST /api/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Yeni şifre en az 6 karakter olmalıdır." });

        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var success = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

        if (!success)
            return BadRequest(new { message = "Mevcut şifre hatalı." });

        return Ok(new { message = "Şifre başarıyla değiştirildi." });
    }
}