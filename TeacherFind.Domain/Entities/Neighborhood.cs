using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities
{
    public class Neighborhood
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Code { get; set; }

        public string Name { get; set; } = default!;

        public Guid DistrictId { get; set; }

        public District District { get; set; } = default!;

        public bool IsActive { get; set; } = true;
    }
}
