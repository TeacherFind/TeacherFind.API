using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Students;

public class StudentLessonHistoryDto
{
    public Guid BookingId { get; set; }
    public Guid TeacherListingId { get; set; }
    public string LessonTitle { get; set; } = default!;
    public Guid TutorUserId { get; set; }
    public string TutorName { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = default!;
    public bool HasReview { get; set; }
}