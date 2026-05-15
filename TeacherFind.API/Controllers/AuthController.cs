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
    private readonly IEmailService _emailService;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IVerificationRepository verificationRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _authService = authService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
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
            RoleValue = (int)user.Role,
            AvatarUrl = user.ProfileImageUrl,
            PhoneNumber = user.PhoneNumber,
            CityId = user.CityId,
            CityName = user.City?.Name
        });
    }

    // POST /api/auth/verify-phone
    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyDto dto)
    {
        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code, "Phone");
        if (code == null)
            return BadRequest(new { message = "Kod hatalı veya süresi dolmuş." });

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        code.IsUsed = true;
        user.IsPhoneVerified = true;
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Telefon doğrulandı" });
    }

    // POST /api/auth/verify-email
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyDto dto)
    {
        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code, "Email");
        if (code == null)
            return BadRequest(new { message = "Kod hatalı veya süresi dolmuş." });

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        code.IsUsed = true;
        user.IsEmailVerified = true;
        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "E-posta doğrulandı" });
    }

    // POST /api/auth/resend-email-verification
    [HttpPost("resend-email-verification")]
    public async Task<IActionResult> ResendEmailVerification(
        [FromBody] ResendEmailVerificationDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        // Always return 200 — don't expose whether email exists
        if (user == null)
            return Ok(new { message = "Eğer bu e-posta kayıtlıysa doğrulama kodu gönderildi." });

        if (user.IsEmailVerified)
            return BadRequest(new { message = "E-posta zaten doğrulanmış." });

        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = Random.Shared.Next(100000, 999999).ToString(),
            Type = "Email",
            ExpireAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "Özel Ders Burada E-posta Doğrulama Kodunuz",
            $"""
            <h2>E-posta Doğrulama</h2>
            <p>Merhaba,</p>
            <p>Özel Ders Burada hesabınızı doğrulamak için aşağıdaki kodu kullanın:</p>
            <h1>{code.Code}</h1>
            <p>Bu kod 15 dakika geçerlidir.</p>
            """);

        return Ok(new { message = "Eğer bu e-posta kayıtlıysa doğrulama kodu gönderildi." });
    }

    // POST /api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null)
            return Ok(new { message = "Eğer bu e-posta kayıtlıysa sıfırlama kodu gönderildi." });

        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = Random.Shared.Next(100000, 999999).ToString(),
            Type = "PasswordReset",
            ExpireAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "Özel Ders Burada Şifre Sıfırlama Kodunuz",
            $"""
            <h2>Şifre Sıfırlama</h2>
            <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>
            <h1>{code.Code}</h1>
            <p>Bu kod 15 dakika geçerlidir.</p>
            """);

        return Ok(new { message = "Sıfırlama kodu e-posta adresinize gönderildi." });
    }

    // POST /api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Geçersiz istek." });

        var code = await _verificationRepository.GetValidCode(user.Id, dto.Code, "PasswordReset");
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
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Yeni şifre en az 6 karakter olmalıdır." });

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var success = await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        if (!success)
            return BadRequest(new { message = "Mevcut şifre hatalı." });

        return Ok(new { message = "Şifre başarıyla değiştirildi." });
    }
}