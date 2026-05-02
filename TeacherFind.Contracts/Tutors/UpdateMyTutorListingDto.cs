using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Tutors;

public class UpdateMyTutorListingDto
{
    public int? SubjectId { get; set; }

    public Guid? CityId { get; set; }

    public Guid? DistrictId { get; set; }

    public Guid? NeighborhoodId { get; set; }

    public string? Headline { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string SubCategory { get; set; } = default!;

    public ServiceType ServiceType { get; set; }

    public int LessonDuration { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
