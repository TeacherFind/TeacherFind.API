using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Services.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;

    public AdminDashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var totalUsers = await _context.Users.CountAsync();

        var totalStudents = await _context.Users
            .CountAsync(x => x.Role == UserRole.Student);

        var totalTutors = await _context.Users
            .CountAsync(x => x.Role == UserRole.Tutor);

        var totalAdmins = await _context.Users
            .CountAsync(x => x.Role == UserRole.Admin);

        var totalSuperAdmins = await _context.Users
            .CountAsync(x => x.Role == UserRole.SuperAdmin);

        var activeUsers = await _context.Users
            .CountAsync(x => x.IsActive);

        var inactiveUsers = await _context.Users
            .CountAsync(x => !x.IsActive);

        var totalListings = await _context.TeacherListings.CountAsync();

        var pendingListings = await _context.TeacherListings
            .CountAsync(x => x.Status == "Pending" || !x.IsApproved);

        var approvedListings = await _context.TeacherListings
            .CountAsync(x => x.IsApproved);

        var activeListings = await _context.TeacherListings
            .CountAsync(x => x.IsActive);

        var inactiveListings = await _context.TeacherListings
            .CountAsync(x => !x.IsActive);

        var totalReviews = await _context.Reviews.CountAsync();

        var totalMessages = await _context.Messages.CountAsync();

        var totalNotifications = await _context.Notifications.CountAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalStudents = totalStudents,
            TotalTutors = totalTutors,
            TotalAdmins = totalAdmins,
            TotalSuperAdmins = totalSuperAdmins,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            TotalListings = totalListings,
            PendingListings = pendingListings,
            ApprovedListings = approvedListings,
            ActiveListings = activeListings,
            InactiveListings = inactiveListings,
            TotalReviews = totalReviews,
            TotalMessages = totalMessages,
            TotalNotifications = totalNotifications
        };
    }
}