using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Bookings;

public class CancelBookingRequestDto
{
    public string? Reason { get; set; }
}