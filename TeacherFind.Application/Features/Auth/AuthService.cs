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
    private readonly ITeacherRepository _teacherRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        ITeacherRepository teacherRepository,
        IVerificationRepository verificationRepository,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _teacherRepository = teacherRepository;
        _verificationRepository = verificationRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null)
            throw new Exception("User already exists");

        var role = ResolveRegisterRole(request.Role);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = true,
            IsEmailVerified = false,
            IsPhoneVerified = false
        };

        await _userRepository.AddAsync(user);

        if (user.Role == UserRole.Tutor)
        {
            var teacherProfile = CreateInitialTeacherProfile(user);
            await _teacherRepository.AddAsync(teacherProfile);
        }

        var verificationCode = CreatePhoneVerificationCode(user.Id);
        await _verificationRepository.AddAsync(verificationCode);

        await _userRepository.SaveChangesAsync();

        Console.WriteLine($"SMS CODE: {verificationCode.Code}");

        return user;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email.Trim());

        if (user is null)
            return null;

        if (!user.IsActive)
            return null;

        var isValidPassword = _passwordHasher.Verify(password, user.PasswordHash);

        if (!isValidPassword)
            return null;

        var token = _jwtProvider.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    private static UserRole ResolveRegisterRole(UserRole requestedRole)
    {
        return requestedRole == UserRole.Tutor
            ? UserRole.Tutor
            : UserRole.Student;
    }

    private static TeacherProfile CreateInitialTeacherProfile(User user)
    {
        return new TeacherProfile
        {
            UserId = user.Id,
            Title = $"{user.FullName} öğretmen profili",
            Headline = null,
            Bio = null,
            TeachingStyle = null,
            City = null,
            Rating = 0,
            TotalReviews = 0,
            EducationLevel = null,
            IsStudent = false
        };
    }

    private static VerificationCode CreatePhoneVerificationCode(Guid userId)
    {
        return new VerificationCode
        {
            UserId = userId,
            Code = Random.Shared.Next(100000, 999999).ToString(),
            Type = "Phone",
            ExpireAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };
    }
}