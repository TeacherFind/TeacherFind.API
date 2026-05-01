using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class MyStudentDto
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = default!;

    public Guid TeacherListingId { get; set; }

    public string LessonTitle { get; set; } = default!;

    public DateTime LastLessonDate { get; set; }

    public string Source { get; set; } = default!;

    public int CompletedLessonCount { get; set; }
}
