using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using TeacherFind.Application.Abstractions.Identity;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Auth;
using TeacherFind.Domain.Entities;
using System.Security.Cryptography;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const int VerificationCodeExpireMinutes = 10;

    private const string InvalidLoginMessage = "Email veya şifre hatalı";
    private const string InvalidCodeMessage = "Kod hatalı veya süresi dolmuş.";

    private const string GenericEmailVerificationMessage =
        "Eğer bu e-posta kayıtlıysa doğrulama kodu gönderildi.";

    private const string GenericPasswordResetMessage =
        "Eğer bu e-posta kayıtlıysa sıfırlama kodu gönderildi.";

    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IVerificationRepository verificationRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    [EnableRateLimiting("register-limit")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request is null)
            return BadRequest(new { message = "Kayıt bilgileri gönderilmedi." });

        try
        {
            var user = await _authService.RegisterAsync(request);

            try
            {
                await CreateAndSendEmailVerificationCodeAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Register başarılı oldu ama e-posta doğrulama kodu gönderilemedi. UserId: {UserId}",
                    user.Id);
            }

            return Ok(new
            {
                message = "Kayıt başarılı. Lütfen e-posta veya telefon doğrulaması yapın.",
                userId = user.Id,
                requiresVerification = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null)
            return BadRequest(new { message = "Giriş bilgileri gönderilmedi." });

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { message = InvalidLoginMessage });
        }

        var email = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = InvalidLoginMessage });

        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
            return Unauthorized(new { message = InvalidLoginMessage });

        if (!user.IsEmailVerified && !user.IsPhoneVerified)
        {
            return Unauthorized(new
            {
                message = "Hesabınızı kullanmadan önce e-posta veya telefon doğrulaması yapmanız gerekiyor.",
                requiresVerification = true,
                userId = user.Id
            });
        }

        var result = await _authService.LoginAsync(email, request.Password, request.RememberMe);

        if (result is null)
            return Unauthorized(new { message = InvalidLoginMessage });

        return Ok(result);
    }

    // GET /api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Geçersiz token" });

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
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
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "Doğrulama bilgileri gönderilmedi." });

        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code, "Phone");

        if (code is null)
            return BadRequest(new { message = InvalidCodeMessage });

        var user = await _userRepository.GetByIdAsync(dto.UserId);

        if (user is null)
            return BadRequest(new { message = InvalidCodeMessage });

        code.IsUsed = true;
        user.IsPhoneVerified = true;

        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Telefon doğrulandı" });
    }

    // POST /api/auth/verify-email
    [HttpPost("verify-email")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "Doğrulama bilgileri gönderilmedi." });

        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code, "Email");

        if (code is null)
            return BadRequest(new { message = InvalidCodeMessage });

        var user = await _userRepository.GetByIdAsync(dto.UserId);

        if (user is null)
            return BadRequest(new { message = InvalidCodeMessage });

        code.IsUsed = true;
        user.IsEmailVerified = true;

        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "E-posta doğrulandı" });
    }

    // POST /api/auth/resend-email-verification
    [HttpPost("resend-email-verification")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ResendEmailVerification(
        [FromBody] ResendEmailVerificationDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "E-posta adresi zorunludur." });

        var email = NormalizeEmail(dto.Email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null || user.IsEmailVerified)
            return Ok(new { message = GenericEmailVerificationMessage });

        try
        {
            await CreateAndSendEmailVerificationCodeAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "E-posta doğrulama kodu gönderilemedi. UserId: {UserId}",
                user.Id);
        }

        return Ok(new { message = GenericEmailVerificationMessage });
    }

    // POST /api/auth/forgot-password
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "E-posta adresi zorunludur." });

        var email = NormalizeEmail(dto.Email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is not null && user.IsActive)
        {
            try
            {
                await CreateAndSendPasswordResetCodeAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Şifre sıfırlama kodu gönderilemedi. UserId: {UserId}",
                    user.Id);
            }
        }

        return Ok(new { message = GenericPasswordResetMessage });
    }

    // POST /api/auth/reset-password
    [HttpPost("reset-password")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "Şifre sıfırlama bilgileri gönderilmedi." });

        if (string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Code) ||
            string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { message = InvalidCodeMessage });
        }

        if (dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Şifre en az 6 karakter olmalıdır." });

        var email = NormalizeEmail(dto.Email);
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
            return BadRequest(new { message = InvalidCodeMessage });

        var code = await _verificationRepository.GetValidCode(user.Id, dto.Code, "PasswordReset");

        if (code is null)
            return BadRequest(new { message = InvalidCodeMessage });

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;
        code.IsUsed = true;

        await _userRepository.SaveChangesAsync();

        return Ok(new { message = "Şifreniz başarıyla değiştirildi." });
    }

    // POST /api/auth/change-password
    [Authorize]
    [HttpPost("change-password")]
    [EnableRateLimiting("auth-limit")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            return BadRequest(new { message = "Mevcut şifre zorunludur." });

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Yeni şifre en az 6 karakter olmalıdır." });

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Geçersiz token." });

        var success = await _authService.ChangePasswordAsync(
            userId,
            dto.CurrentPassword,
            dto.NewPassword);

        if (!success)
            return BadRequest(new { message = "Mevcut şifre hatalı." });

        return Ok(new { message = "Şifre başarıyla değiştirildi." });
    }

    // =====================================================
    // Private Helpers
    // =====================================================

    private async Task CreateAndSendEmailVerificationCodeAsync(User user)
    {
        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = GenerateSixDigitCode(),
            Type = "Email",
            ExpireAt = DateTime.UtcNow.AddMinutes(VerificationCodeExpireMinutes),
            IsUsed = false
        };

        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "Özel Ders VIP E-posta Doğrulama Kodunuz",
            $"""
            <h2>E-posta Doğrulama</h2>
            <p>Merhaba,</p>
            <p>Özel Ders VIP hesabınızı doğrulamak için aşağıdaki kodu kullanın:</p>
            <h1>{code.Code}</h1>
            <p>Bu kod {VerificationCodeExpireMinutes} dakika geçerlidir.</p>
            """);
    }

    private async Task CreateAndSendPasswordResetCodeAsync(User user)
    {
        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = GenerateSixDigitCode(),
            Type = "PasswordReset",
            ExpireAt = DateTime.UtcNow.AddMinutes(VerificationCodeExpireMinutes),
            IsUsed = false
        };

        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "Özel Ders VIP Şifre Sıfırlama Kodunuz",
            $"""
            <h2>Şifre Sıfırlama</h2>
            <p>Şifrenizi sıfırlamak için aşağıdaki kodu kullanın:</p>
            <h1>{code.Code}</h1>
            <p>Bu kod {VerificationCodeExpireMinutes} dakika geçerlidir.</p>
            """);
    }

    private static string GenerateSixDigitCode()
        => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private static string NormalizeEmail(string email)
        => email.Trim();
}