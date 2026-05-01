using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Bookings;

public class CreateBookingRequestDto
{
    public Guid TeacherListingId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public BookingSource Source { get; set; } = BookingSource.Site;

    public string? StudentNote { get; set; }
}
