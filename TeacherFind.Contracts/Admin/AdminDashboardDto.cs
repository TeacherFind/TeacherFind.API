using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }

    public int TotalStudents { get; set; }

    public int TotalTutors { get; set; }

    public int TotalAdmins { get; set; }

    public int TotalSuperAdmins { get; set; }

    public int ActiveUsers { get; set; }

    public int InactiveUsers { get; set; }

    public int TotalListings { get; set; }

    public int PendingListings { get; set; }

    public int ApprovedListings { get; set; }

    public int ActiveListings { get; set; }

    public int InactiveListings { get; set; }

    public int TotalReviews { get; set; }

    public int TotalMessages { get; set; }

    public int TotalNotifications { get; set; }
}
