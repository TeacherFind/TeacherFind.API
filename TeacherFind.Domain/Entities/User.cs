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

    public UserRole Role { get; set; } = UserRole.Student;

    public bool IsEmailVerified { get; set; } = false;

    public bool IsPhoneVerified { get; set; } = false;

    public DateTime? LastLoginAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Relations
    public TeacherProfile? TeacherProfile { get; set; }

    public ICollection<TeacherListing> TeacherListings { get; set; } = new List<TeacherListing>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();

    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}