using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Application.Abstractions.Repositories;
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

    [HttpPost("verify-phone")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyDto dto)
    {
        var code = await _verificationRepository.GetValidCode(dto.UserId, dto.Code);

        if (code == null)
            return BadRequest("Kod yanlış");

        var user = await _userRepository.GetByIdAsync(dto.UserId);

        if (user == null)
            return NotFound("Kullanıcı bulunamadı");

        user.IsPhoneVerified = true;

        await _userRepository.SaveChangesAsync();

        return Ok("Telefon doğrulandı");
    }
}