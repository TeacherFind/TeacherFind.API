using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminDashboardController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var now = DateTime.UtcNow;
        var last30 = now.AddDays(-30).Date;

        var totalUsers = await _context.Users.CountAsync();
        var totalTutors = await _context.Users.CountAsync(u => u.Role == UserRole.Tutor);
        var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        var totalListings = await _context.TeacherListings.CountAsync();
        var activeListings = await _context.TeacherListings.CountAsync(x => x.IsActive && x.IsApproved);
        var pendingListings = await _context.TeacherListings.CountAsync(x => x.IsActive && !x.IsApproved);
        var totalReviews = await _context.Reviews.CountAsync();
        var totalReports = await _context.Reports.CountAsync();
        var pendingReports = await _context.Reports.CountAsync(r => r.Status == "Pending");

        var registrations = await _context.Users
            .Where(u => u.CreatedAt >= last30)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var registrationSeries = Enumerable.Range(0, 30)
            .Select(i => last30.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = registrations.FirstOrDefault(r => r.Date == date)?.Count ?? 0
            }).ToList();

        var messages = await _context.Messages
            .Where(m => m.SentAt >= last30)
            .GroupBy(m => m.SentAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var messageSeries = Enumerable.Range(0, 30)
            .Select(i => last30.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = messages.FirstOrDefault(m => m.Date == date)?.Count ?? 0
            }).ToList();

        var listings = await _context.TeacherListings
            .Where(l => l.CreatedAt >= last30)
            .GroupBy(l => l.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var listingSeries = Enumerable.Range(0, 30)
            .Select(i => last30.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = listings.FirstOrDefault(l => l.Date == date)?.Count ?? 0
            }).ToList();

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalTutors = totalTutors,
            TotalStudents = totalStudents,
            TotalListings = totalListings,
            ActiveListings = activeListings,
            PendingListings = pendingListings,
            TotalReviews = totalReviews,
            TotalReports = totalReports,
            PendingReports = pendingReports,
            RegistrationsLast30Days = registrationSeries,
            MessageTrafficLast30Days = messageSeries,
            NewListingsLast30Days = listingSeries
        });
    }
}