using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Students;

public class StudentProfileDto
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string? PhoneNumber { get; set; }

    public Guid? CityId { get; set; }

    public string? CityName { get; set; }

    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }
}