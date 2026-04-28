using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Auth;

public class LoginResponse
{
    public string Token { get; set; } = default!;

    public Guid UserId { get; set; }

    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Role { get; set; } = default!;
}