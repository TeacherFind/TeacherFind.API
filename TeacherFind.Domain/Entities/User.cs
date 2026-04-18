using TeacherFind.Domain.Common;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Domain.Entities;

public class User : AuditableEntity
{
    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public string? PhoneNumber { get; set; }

    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }

    public Gender Gender { get; set; } = Gender.NotSpecified;

    public bool IsActive { get; set; } = true;

    public string Role { get; set; } = "User";

    // Relations
    public TeacherProfile? TeacherProfile { get; set; }
}