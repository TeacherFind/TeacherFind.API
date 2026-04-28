using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AdminInvitationCreatedResponse
{
    public Guid InvitationId { get; set; }

    public string Email { get; set; } = default!;

    public string Role { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    public string Token { get; set; } = default!;

    public string InvitationUrl { get; set; } = default!;
}