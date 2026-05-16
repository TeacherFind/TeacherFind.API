using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminDashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var now = DateTime.UtcNow;
        var last30Days = now.AddDays(-30).Date;
        var previous30DaysStart = now.AddDays(-60).Date;

        var totalUsers = await _context.Users.CountAsync();

        var totalTutors = await _context.Users.CountAsync(x =>
            x.Role == UserRole.Tutor);

        var totalStudents = await _context.Users.CountAsync(x =>
            x.Role == UserRole.Student);

        var totalMessages = await _context.Messages.CountAsync();

        var totalReviews = await _context.Reviews.CountAsync();

        var pendingListings = await _context.TeacherListings.CountAsync(x =>
            !x.IsApproved &&
            x.IsActive &&
            x.Status == "PendingApproval");

        var activeListings = await _context.TeacherListings.CountAsync(x =>
            x.IsApproved &&
            x.IsActive &&
            x.Status == "Active");

        var revenue = await _context.Bookings
            .Where(x => x.Status == BookingStatus.Completed)
            .Include(x => x.TeacherListing)
            .SumAsync(x => x.TeacherListing.Price);

        var currentPeriodUsers = await _context.Users.CountAsync(x =>
            x.CreatedAt >= last30Days);

        var previousPeriodUsers = await _context.Users.CountAsync(x =>
            x.CreatedAt >= previous30DaysStart &&
            x.CreatedAt < last30Days);

        double monthlyGrowth;

        if (previousPeriodUsers == 0)
        {
            monthlyGrowth = currentPeriodUsers > 0 ? 100 : 0;
        }
        else
        {
            monthlyGrowth = Math.Round(
                ((currentPeriodUsers - previousPeriodUsers) / (double)previousPeriodUsers) * 100,
                2);
        }

        var registrations = await _context.Users
            .Where(x => x.CreatedAt >= last30Days)
            .GroupBy(x => x.CreatedAt.Date)
            .Select(x => new
            {
                Date = x.Key,
                Count = x.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var registrationsLast30Days = Enumerable.Range(0, 30)
            .Select(i => last30Days.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = registrations.FirstOrDefault(x => x.Date == date)?.Count ?? 0
            })
            .ToList();

        var messageTraffic = await _context.Messages
            .Where(x => x.SentAt >= last30Days)
            .GroupBy(x => x.SentAt.Date)
            .Select(x => new
            {
                Date = x.Key,
                Count = x.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var messageTrafficLast30Days = Enumerable.Range(0, 30)
            .Select(i => last30Days.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = messageTraffic.FirstOrDefault(x => x.Date == date)?.Count ?? 0
            })
            .ToList();

        var newListings = await _context.TeacherListings
            .Where(x => x.CreatedAt >= last30Days)
            .GroupBy(x => x.CreatedAt.Date)
            .Select(x => new
            {
                Date = x.Key,
                Count = x.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var newListingsLast30Days = Enumerable.Range(0, 30)
            .Select(i => last30Days.AddDays(i))
            .Select(date => new
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = newListings.FirstOrDefault(x => x.Date == date)?.Count ?? 0
            })
            .ToList();

        return Ok(new
        {
            totalUsers,
            totalTutors,
            totalStudents,
            pendingListings,
            activeListings,
            totalMessages,
            totalReviews,
            revenue,
            monthlyGrowth,
            registrationsLast30Days,
            messageTrafficLast30Days,
            newListingsLast30Days
        });
    }
}