using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Students;

public class UpdateStudentProfileDto
{
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public Guid? CityId { get; set; }

    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; } = null;
}