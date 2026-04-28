using TeacherFind.Domain.Common;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Domain.Entities;

public class AdminInvitation : AuditableEntity
{
    public string Email { get; set; } = default!;

    public UserRole Role { get; set; } = UserRole.Admin;

    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime? UsedAt { get; set; }

    public Guid InvitedByUserId { get; set; }

    public User InvitedByUser { get; set; } = default!;
}