using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Auth;

public class VerifyDto
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = default!;
}