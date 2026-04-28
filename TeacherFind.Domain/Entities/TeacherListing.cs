using TeacherFind.Domain.Common;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Domain.Entities;

public class TeacherListing : AuditableEntity
{
    public Guid TeacherProfileId { get; set; }
    public TeacherProfile TeacherProfile { get; set; } = default!;

    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid? CityId { get; set; }
    public City? City { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public Guid? NeighborhoodId { get; set; }
    public Neighborhood? Neighborhood { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string SubCategory { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public bool IsApproved { get; set; } = false;

    public ServiceType ServiceType { get; set; }

    public int LessonDuration { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = "Pending";

    public int ViewCount { get; set; } = 0;
}