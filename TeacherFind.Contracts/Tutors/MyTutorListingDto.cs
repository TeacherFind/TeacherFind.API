using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class MyTutorListingDto
{
    public Guid Id { get; set; }

    public Guid TeacherProfileId { get; set; }

    public int? SubjectId { get; set; }

    public string? SubjectName { get; set; }

    public Guid? CityId { get; set; }

    public string? CityName { get; set; }

    public Guid? DistrictId { get; set; }

    public string? DistrictName { get; set; }

    public Guid? NeighborhoodId { get; set; }

    public string? NeighborhoodName { get; set; }

    public string? Headline { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string SubCategory { get; set; } = default!;

    public string ServiceType { get; set; } = default!;

    public int LessonDuration { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = default!;

    public bool IsActive { get; set; }

    public bool IsApproved { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public List<ListingPhotoDto>? Photos { get; set; }
}