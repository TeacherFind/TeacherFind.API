using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AdminListingDto
{
    public Guid Id { get; set; }

    public Guid TeacherProfileId { get; set; }

    public string TeacherName { get; set; } = default!;

    public string? TeacherEmail { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string SubCategory { get; set; } = default!;

    public string? SubjectName { get; set; }

    public string? CityName { get; set; }

    public string? DistrictName { get; set; }

    public string? NeighborhoodName { get; set; }

    public string ServiceType { get; set; } = default!;

    public int LessonDuration { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = default!;

    public bool IsActive { get; set; }

    public bool IsApproved { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }
}