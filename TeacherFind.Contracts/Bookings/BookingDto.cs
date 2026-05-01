using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Bookings;

public class BookingDto
{
    public Guid Id { get; set; }

    public Guid TeacherListingId { get; set; }

    public string LessonTitle { get; set; } = default!;

    public Guid StudentUserId { get; set; }

    public string StudentName { get; set; } = default!;

    public Guid TutorUserId { get; set; }

    public string TutorName { get; set; } = default!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = default!;

    public string Source { get; set; } = default!;

    public string? StudentNote { get; set; }

    public string? TutorNote { get; set; }

    public DateTime CreatedAt { get; set; }
}
