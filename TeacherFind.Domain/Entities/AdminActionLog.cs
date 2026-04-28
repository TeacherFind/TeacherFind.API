using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities;

public class AdminActionLog : AuditableEntity
{
    public Guid AdminUserId { get; set; }

    public User AdminUser { get; set; } = default!;

    public string Action { get; set; } = default!;

    public string EntityName { get; set; } = default!;

    public Guid? EntityId { get; set; }

    public string? Description { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}