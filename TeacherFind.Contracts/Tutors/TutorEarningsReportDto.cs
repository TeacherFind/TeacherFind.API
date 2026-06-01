using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TeacherFind.Contracts.Tutors;

public class TutorEarningsReportDto
{
    public decimal TotalEarnings { get; set; }
    public int CompletedLessonCount { get; set; }
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public List<TutorEarningsItemDto> Items { get; set; } = new();
}

public class TutorEarningsItemDto
{
    public Guid BookingId { get; set; }
    public string LessonTitle { get; set; } = default!;
    public string StudentName { get; set; } = default!;
    public DateTime CompletedAt { get; set; }
    public decimal Amount { get; set; }
}