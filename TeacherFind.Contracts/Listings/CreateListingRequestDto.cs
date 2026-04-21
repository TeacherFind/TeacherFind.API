using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Listings;

public class CreateListingRequestDto
{
    public Guid TeacherProfileId { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;

    public string Category { get; set; } = default!;
    public string SubCategory { get; set; } = default!;

    public int LessonDuration { get; set; }

    public decimal Price { get; set; }
    public int CityId { get; set; }
    public ServiceType ServiceType { get; set; }
}