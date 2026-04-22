using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities
{
    public class TeacherAvailability : BaseEntity
    {
        public Guid TeacherProfileId { get; set; }
        public string Day { get; set; } = default!;
        public string Start { get; set; } = default!;
        public string End { get; set; } = default!;
    }
}
