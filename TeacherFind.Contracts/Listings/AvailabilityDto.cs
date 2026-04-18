using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Listings
{
    public class AvailabilityDto
    {
        public string Gun { get; set; } = default!;
        public string Baslangic { get; set; } = default!;
        public string Bitis { get; set; } = default!;
    }
}
