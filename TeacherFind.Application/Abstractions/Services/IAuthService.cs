using TeacherFind.Contracts.Auth;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterRequest request);

    Task<LoginResponse?> LoginAsync(string email, string password);
}