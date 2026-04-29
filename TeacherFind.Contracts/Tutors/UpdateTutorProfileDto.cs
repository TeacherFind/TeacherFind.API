using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class UpdateTutorProfileDto
{
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? TeachingStyle { get; set; }
    public string? City { get; set; }
    public Guid? UniversityId { get; set; }
    public Guid? DepartmentId { get; set; }
}