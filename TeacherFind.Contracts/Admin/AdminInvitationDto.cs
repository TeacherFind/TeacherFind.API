using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AdminInvitationDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = default!;

    public string Role { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public Guid InvitedByUserId { get; set; }

    public string InvitedByFullName { get; set; } = default!;

    public string InvitedByEmail { get; set; } = default!;
}
