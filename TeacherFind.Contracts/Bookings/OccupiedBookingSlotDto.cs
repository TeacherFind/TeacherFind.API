using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Bookings;

public class OccupiedBookingSlotDto
{
    public Guid BookingId { get; set; }

    public Guid TeacherListingId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = default!;
}