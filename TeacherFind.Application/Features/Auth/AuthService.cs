using TeacherFind.Application.Abstractions.Identity;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Auth;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;

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

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);

        if (existing != null)
            throw new Exception("Bu e-posta adresi zaten kullanılıyor.");

        var role = request.Role == UserRole.Tutor
            ? UserRole.Tutor
            : UserRole.Student;

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
        ? null
        : request.PhoneNumber.Trim(),
            CityId = request.CityId,
            IsActive = true,
            IsEmailVerified = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var code = new VerificationCode
        {
            UserId = user.Id,
            Code = new Random().Next(100000, 999999).ToString(),
            Type = "Phone",
            ExpireAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _verificationRepository.AddAsync(code);
        await _verificationRepository.SaveChangesAsync();

        Console.WriteLine($"[VERIFICATION CODE] {user.Email} -> {code.Code}");

        return user;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !user.IsActive)
            return null;

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            return null;

        var token = _jwtProvider.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            PhoneNumber = user.PhoneNumber,
            CityId = user.CityId,
            CityName = user.City?.Name
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        if (!_passwordHasher.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}