using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class CreateAdminInvitationRequest
{
    public string Email { get; set; } = default!;

    public string Role { get; set; } = "Admin";
}
