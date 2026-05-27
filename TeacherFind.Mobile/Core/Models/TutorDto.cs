using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Mobile.Core.Models
{
    public class TutorDto
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Subject { get; set; }

        public decimal Price { get; set; }
    }
}
