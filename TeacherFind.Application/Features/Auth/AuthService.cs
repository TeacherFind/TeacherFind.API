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
    private readonly ITeacherRepository _teacherRepository;

    public AuthService(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher,
        ITeacherRepository teacherRepository)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _teacherRepository = teacherRepository;
    }

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new Exception("Ad soyad zorunludur.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new Exception("E-posta adresi zorunludur.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new Exception("Şifre en az 6 karakter olmalıdır.");

        var normalizedEmail = NormalizeEmail(request.Email);

        var existing = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (existing is not null)
            throw new Exception("Bu e-posta adresi zaten kullanılıyor.");

        var role = request.Role == UserRole.Tutor
            ? UserRole.Tutor
            : UserRole.Student;

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim(),
            CityId = request.CityId,
            IsActive = true,
            IsEmailVerified = true,
            IsPhoneVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        if (role == UserRole.Tutor)
        {
            var teacherProfile = CreateTeacherProfile(user, request);

            await _teacherRepository.AddAsync(teacherProfile);
            await _teacherRepository.SaveChangesAsync();
        }

        return user;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var normalizedEmail = NormalizeEmail(email);

        var user = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (user is null)
            return null;

        if (!user.IsActive)
            return null;

        var isPasswordValid = _passwordHasher.Verify(
            password,
            user.PasswordHash);

        if (!isPasswordValid)
            return null;

        if (!user.IsEmailVerified && !user.IsPhoneVerified)
            return null;

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

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            return false;

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return false;

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
            return false;

        var isCurrentPasswordValid = _passwordHasher.Verify(
            currentPassword,
            user.PasswordHash);

        if (!isCurrentPasswordValid)
            return false;

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        return true;
    }

    private static TeacherProfile CreateTeacherProfile(
        User user,
        RegisterRequest request)
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

        if (request.Certificates is not null)
        {
            foreach (var cert in request.Certificates)
            {
                if (string.IsNullOrWhiteSpace(cert.Name))
                    continue;

                teacherProfile.Certificates.Add(new TeacherCertificate
                {
                    Name = cert.Name.Trim(),
                    Organization = cert.Organization?.Trim() ?? string.Empty,
                    Year = cert.Year ?? DateTime.UtcNow.Year,
                    FileUrl = cert.FileUrl
                });
            }
        }

        if (request.Subjects is not null)
        {
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
        }

        return teacherProfile;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}