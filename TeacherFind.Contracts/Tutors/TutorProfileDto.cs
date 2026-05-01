using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class TutorProfileDto
{
    public Guid UserId { get; set; }
    public Guid TeacherProfileId { get; set; }

    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }

    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }

    public Guid? UniversityId { get; set; }
    public string? UniversityName { get; set; }

    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    public string? EducationLevel { get; set; }
    public bool? IsStudent { get; set; }
    public string? TeachingStyle { get; set; }

    public double Rating { get; set; }
    public int TotalReviews { get; set; }

    public List<TutorProfileCertificateDto> Certificates { get; set; } = new();
    public List<TutorProfileAvailabilityDto> Availabilities { get; set; } = new();
}

public class TutorProfileCertificateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Organization { get; set; } = default!;
    public int Year { get; set; }
}

public class TutorProfileAvailabilityDto
{
    public Guid Id { get; set; }
    public string Day { get; set; } = default!;
    public string Start { get; set; } = default!;
    public string End { get; set; } = default!;
}