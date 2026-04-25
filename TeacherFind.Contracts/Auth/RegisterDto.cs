using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Auth;

public class RegisterDto
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;

    /// <summary>Allowed values: "Student" or "Tutor"</summary>
    public string Role { get; set; } = "Student";
}