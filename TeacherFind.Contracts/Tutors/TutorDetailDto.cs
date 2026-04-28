namespace TeacherFind.Contracts.Tutors;

public class TutorDetailDto
{
    public Guid Id { get; set; }
    public Guid TeacherProfileId { get; set; }
    public string TeacherName { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public string Title { get; set; } = default!;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? TeachingStyle { get; set; }
    public decimal Price { get; set; }
    public int LessonDuration { get; set; }
    public string ServiceType { get; set; } = default!;
    public string? Subject { get; set; }
    public string? Category { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Neighborhood { get; set; }
    public string? University { get; set; }
    public string? Department { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public int ViewCount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<TutorReviewDto> Reviews { get; set; } = new();
    public List<TutorAvailabilityDto> Availability { get; set; } = new();
    public List<TutorCertificateDto> Documents { get; set; } = new();
}