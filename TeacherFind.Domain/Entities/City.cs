using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities;

public class City
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int PlateCode { get; set; }

    public string Name { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public ICollection<District> Districts { get; set; } = new List<District>();
}
