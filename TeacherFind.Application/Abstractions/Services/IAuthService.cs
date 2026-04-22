using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(string fullName, string email, string password);

    Task<string?> LoginAsync(string email, string password); 
}