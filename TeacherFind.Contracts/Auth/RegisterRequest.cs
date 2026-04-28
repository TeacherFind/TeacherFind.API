using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Auth;

public class RegisterRequest
{
    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Password { get; set; } = default!;

    public UserRole Role { get; set; } = UserRole.Student;
}
