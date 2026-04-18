using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities
{
    public class Favorite
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid ListingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
