using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class TutorCertificateDto
{
    public string Name { get; set; } = default!;
    public string Organization { get; set; } = default!;
    public int Year { get; set; }
}