using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AcceptAdminInvitationRequest
{
    public string Token { get; set; } = default!;

    public string FullName { get; set; } = default!;

    public string Password { get; set; } = default!;
}