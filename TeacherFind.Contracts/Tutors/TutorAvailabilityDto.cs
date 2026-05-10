using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class TutorAvailabilityDto
{
    public string Day { get; set; } = default!;
    public string Start { get; set; } = default!;
    public string End { get; set; } = default!;
    public string? Type { get; set; }
}