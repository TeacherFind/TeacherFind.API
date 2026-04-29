using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Students;

public class StudentDashboardStatsDto
{
    public int UpcomingLessons { get; set; }

    public int CompletedLessons { get; set; }

    public int PendingBookings { get; set; }

    public int CancelledLessons { get; set; }

    public int TotalTutorsContacted { get; set; }

    public int RemainingSubscriptionHours { get; set; } = 0;
}
