using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Application.Abstractions.Identity;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> RegisterAsync(string fullName, string email, string password)
    {
        var existing = await _userRepository.GetByEmailAsync(email);

        if (existing != null)
            throw new Exception("User already exists");

        var hashedPassword = _passwordHasher.Hash(password);

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PasswordHash = hashedPassword // 🔥 HASHED
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
            return null;

        var isValid = _passwordHasher.Verify(password, user.PasswordHash);

        if (!isValid)
            return null;

        var token = _jwtProvider.GenerateToken(user);

        return token;
    }
}