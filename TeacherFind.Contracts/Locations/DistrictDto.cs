using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Locations;

public class DistrictDto
{
    public Guid Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; } = default!;
}