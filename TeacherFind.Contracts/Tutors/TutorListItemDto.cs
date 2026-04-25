using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class TutorListItemDto
{
    public Guid Id { get; set; }
    public Guid TeacherProfileId { get; set; }
    public string TeacherName { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string ServiceType { get; set; } = default!;
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Neighborhood { get; set; }
    public string? Subject { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsFavorite { get; set; }
}