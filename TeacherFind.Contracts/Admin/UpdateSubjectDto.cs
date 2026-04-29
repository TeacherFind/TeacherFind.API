using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class UpdateSubjectDto
{
    public string Name { get; set; } = default!;
    public string Category { get; set; } = default!;
}