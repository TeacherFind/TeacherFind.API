using TeacherFind.Application.Abstractions.Identity;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IVerificationRepository _verificationRepository;

    public AuthService(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher,
        IVerificationRepository verificationRepository)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _verificationRepository = verificationRepository;
    }

    public async Task<User> RegisterAsync(string fullName, string email, string password, UserRole role)
    {
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new Exception("Bu e-posta adresi zaten kullanılıyor.");

        if (role != UserRole.Student && role != UserRole.Tutor)
            throw new Exception("Geçersiz rol. Sadece Student veya Tutor seçilebilir.");

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            Role = role
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = new Random().Next(100000, 999999).ToString(),
            Type = "Email",
            ExpireAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        Console.WriteLine($"[VERIFICATION CODE] {user.Email} -> {code.Code}");

        return user;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return null;

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync();

        return _jwtProvider.GenerateToken(user);
    }
}