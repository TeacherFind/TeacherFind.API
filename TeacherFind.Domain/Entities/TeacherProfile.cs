using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities;

public class TeacherProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? TeachingStyle { get; set; }
    public string? City { get; set; }

    public double Rating { get; set; }
    public int TotalReviews { get; set; }

    public Guid? UniversityId { get; set; }
    public University? University { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? DepartmentEntity { get; set; }

    public string? EducationLevel { get; set; }
    public bool? IsStudent { get; set; }

    public ICollection<TeacherCertificate> Certificates { get; set; } = new List<TeacherCertificate>();
    public ICollection<TeacherAvailability> Availabilities { get; set; } = new List<TeacherAvailability>();
}