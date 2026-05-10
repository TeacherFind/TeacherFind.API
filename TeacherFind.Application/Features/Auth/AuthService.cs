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
    private readonly ITeacherRepository _teacherRepository;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher,
        IVerificationRepository verificationRepository,
        ITeacherRepository teacherRepository,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _verificationRepository = verificationRepository;
        _teacherRepository = teacherRepository;
        _emailService = emailService;
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
                              ? null : request.PhoneNumber.Trim(),
            CityId = request.CityId,
            IsActive = true,
            IsEmailVerified = false
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        if (role == UserRole.Tutor)
        {
            var teacherProfile = new TeacherProfile
            {
                UserId = user.Id,
                Title = $"{user.FullName} öğretmen profili",
                Bio = request.Bio?.Trim(),
                UniversityId = request.UniversityId,
                DepartmentId = request.DepartmentId,
                Rating = 0,
                TotalReviews = 0,
                IsStudent = false
            };

            foreach (var cert in request.Certificates)
            {
                teacherProfile.Certificates.Add(new TeacherCertificate
                {
                    Name = cert.Name.Trim(),
                    Organization = cert.Organization?.Trim() ?? string.Empty,
                    Year = cert.Year ?? DateTime.UtcNow.Year,
                    FileUrl = cert.FileUrl
                });
            }

            foreach (var sub in request.Subjects)
            {
                teacherProfile.Subjects.Add(new TeacherProfileSubject
                {
                    SubjectId = sub.SubjectId,
                    Stage = sub.Stage?.Trim(),
                    Category = sub.Category?.Trim(),
                    Name = sub.Name?.Trim(),
                    Level = sub.Level?.Trim()
                });
            }

            await _teacherRepository.AddAsync(teacherProfile);
            await _teacherRepository.SaveChangesAsync();
        }

        // Email verification code
        var emailCode = new VerificationCode
        {
            UserId = user.Id,
            Code = Random.Shared.Next(100000, 999999).ToString(),
            Type = "Email",
            ExpireAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _verificationRepository.AddAsync(emailCode);
        await _verificationRepository.SaveChangesAsync();

        await _emailService.SendAsync(
            user.Email,
            "Özel Ders Burada E-posta Doğrulama Kodunuz",
            $"""
            <h2>E-posta Doğrulama</h2>
            <p>Merhaba,</p>
            <p>Özel Ders Burada hesabınızı doğrulamak için aşağıdaki kodu kullanın:</p>
            <h1>{emailCode.Code}</h1>
            <p>Bu kod 15 dakika geçerlidir.</p>
            """);

        return user;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive) return null;
        if (!_passwordHasher.Verify(password, user.PasswordHash)) return null;

        var token = _jwtProvider.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            RoleValue = (int)user.Role,
            PhoneNumber = user.PhoneNumber,
            CityId = user.CityId,
            CityName = user.City?.Name
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;
        if (!_passwordHasher.Verify(currentPassword, user.PasswordHash)) return false;

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _userRepository.SaveChangesAsync();
        return true;
    }
}