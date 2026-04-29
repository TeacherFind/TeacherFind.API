using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Auth;

public class ForgotPasswordDto
{
    public string Email { get; set; } = default!;
}