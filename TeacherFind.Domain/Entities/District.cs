using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities
{
    public class District
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Code { get; set; }

        public string Name { get; set; } = default!;

        public Guid CityId { get; set; }

        public City City { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
    }
}
