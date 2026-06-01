using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;


namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Policy = "AdminOnly")]
public class AdminSettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminSettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await GetOrCreateSettingsAsync();

        return Ok(ToDto(settings));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSystemSettingsRequest request)
    {
        if (request is null)
            return BadRequest(new { message = "Ayar bilgileri gönderilmedi." });

        if (string.IsNullOrWhiteSpace(request.SiteTitle))
            return BadRequest(new { message = "Site başlığı zorunludur." });

        if (string.IsNullOrWhiteSpace(request.ContactEmail))
            return BadRequest(new { message = "İletişim e-postası zorunludur." });

        if (request.CommissionRate < 0 || request.CommissionRate > 100)
            return BadRequest(new { message = "Komisyon oranı 0 ile 100 arasında olmalıdır." });

        if (request.MinWithdrawal < 0)
            return BadRequest(new { message = "Minimum çekim tutarı negatif olamaz." });

        var settings = await GetOrCreateSettingsAsync();

        settings.SiteTitle = request.SiteTitle.Trim();
        settings.ContactEmail = request.ContactEmail.Trim();
        settings.MaintenanceMode = request.MaintenanceMode;
        settings.CommissionRate = request.CommissionRate;
        settings.MinWithdrawal = request.MinWithdrawal;
        settings.SocialLinksJson = SerializeSocialLinks(request.SocialLinks);
        settings.UpdatedAt = DateTime.UtcNow;
        settings.SiteDescription = request.SiteDescription?.Trim();
        settings.SiteKeywords = request.SiteKeywords?.Trim();
        settings.GoogleAnalyticsId = request.GoogleAnalyticsId?.Trim();

        // Merge flat social fields into dictionary
        var socialLinks = new Dictionary<string, string>(request.SocialLinks);
        if (!string.IsNullOrWhiteSpace(request.FacebookLink))
            socialLinks["facebook"] = request.FacebookLink.Trim();
        if (!string.IsNullOrWhiteSpace(request.InstagramLink))
            socialLinks["instagram"] = request.InstagramLink.Trim();
        if (!string.IsNullOrWhiteSpace(request.TwitterLink))
            socialLinks["twitter"] = request.TwitterLink.Trim();
        if (!string.IsNullOrWhiteSpace(request.LinkedInLink))
            socialLinks["linkedin"] = request.LinkedInLink.Trim();
        if (!string.IsNullOrWhiteSpace(request.YoutubeLink))
            socialLinks["youtube"] = request.YoutubeLink.Trim();

        settings.SocialLinksJson = SerializeSocialLinks(socialLinks);

        await _context.SaveChangesAsync();

        return Ok(ToDto(settings));
    }

    private async Task<SystemSetting> GetOrCreateSettingsAsync()
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();

        if (settings is not null)
            return settings;

        settings = new SystemSetting();

        await _context.SystemSettings.AddAsync(settings);
        await _context.SaveChangesAsync();

        return settings;
    }

    private static SystemSettingsDto ToDto(SystemSetting settings)
    {
        var socialLinks = DeserializeSocialLinks(settings.SocialLinksJson);

        return new SystemSettingsDto
        {
            SiteTitle = settings.SiteTitle,
            ContactEmail = settings.ContactEmail,
            MaintenanceMode = settings.MaintenanceMode,
            CommissionRate = settings.CommissionRate,
            MinWithdrawal = settings.MinWithdrawal,
            SiteDescription = settings.SiteDescription,
            SiteKeywords = settings.SiteKeywords,
            GoogleAnalyticsId = settings.GoogleAnalyticsId,
            SocialLinks = socialLinks
        };
    }

    private static string SerializeSocialLinks(Dictionary<string, string>? socialLinks)
    {
        return JsonSerializer.Serialize(socialLinks ?? new Dictionary<string, string>());
    }

    private static Dictionary<string, string> DeserializeSocialLinks(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
    // GET /api/admin/dashboard/metrics — quick analytics
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var now = DateTime.UtcNow;
        var last7 = now.AddDays(-7);
        var last30 = now.AddDays(-30);

        // Student occupancy — % of students with at least 1 completed booking
        var totalStudents = await _context.Users
            .CountAsync(x => x.Role == UserRole.Student);

        var activeStudents = await _context.Bookings
            .Where(x => x.Status == BookingStatus.Completed && x.StartTime >= last30)
            .Select(x => x.StudentUserId)
            .Distinct()
            .CountAsync();

        var studentOccupancyRate = totalStudents > 0
            ? Math.Round((double)activeStudents / totalStudents * 100, 1)
            : 0;

        // Listing approval speed — avg hours from creation to approval
        var approvedLastMonth = await _context.TeacherListings
            .Where(x => x.Status == "Active" && x.UpdatedAt >= last30)
            .Select(x => new { x.CreatedAt, x.UpdatedAt })
            .ToListAsync();

        var avgApprovalHours = approvedLastMonth.Count > 0
            ? Math.Round(approvedLastMonth
                .Where(x => x.UpdatedAt.HasValue)
                .Average(x => (x.UpdatedAt!.Value - x.CreatedAt).TotalHours), 1)
            : 0;

        // Pending reports
        var pendingReports = await _context.Reports
            .CountAsync(x => x.Status == "Pending");

        // New users this week
        var newUsersThisWeek = await _context.Users
            .CountAsync(x => x.CreatedAt >= last7);

        return Ok(new
        {
            studentOccupancyRate,
            avgApprovalHours,
            pendingReports,
            newUsersThisWeek
        });
    }

    // GET /api/admin/dashboard/recent-activities — last 20 platform events
    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivities()
    {
        var activities = new List<object>();

        // New registrations
        var newUsers = await _context.Users
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new
            {
                type = "NewUser",
                message = $"{x.FullName} platforma kayıt oldu.",
                role = x.Role.ToString(),
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        activities.AddRange(newUsers);

        // New listings
        var newListings = await _context.TeacherListings
            .Include(x => x.TeacherProfile).ThenInclude(p => p.User)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new
            {
                type = "NewListing",
                message = $"{x.TeacherProfile.User.FullName} yeni ilan oluşturdu: {x.Title}",
                listingId = x.Id,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        activities.AddRange(newListings);

        // New reports
        var newReports = await _context.Reports
            .Include(x => x.Reporter)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new
            {
                type = "NewReport",
                message = $"{x.Reporter!.FullName} yeni şikayet oluşturdu: {x.Reason}",
                reportId = x.Id,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        activities.AddRange(newReports);

        // New reviews
        var newReviews = await _context.Reviews
            .Include(x => x.Reviewer)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new
            {
                type = "NewReview",
                message = $"{x.Reviewer!.FullName} yorum bıraktı. Puan: {x.Rating}/5",
                rating = x.Rating,
                createdAt = x.CreatedAt
            })
            .ToListAsync();

        activities.AddRange(newReviews);

        var sorted = activities
            .OrderByDescending(x => (DateTime)x.GetType().GetProperty("createdAt")!.GetValue(x)!)
            .Take(20)
            .ToList();

        return Ok(sorted);
    }

    // GET /api/admin/dashboard/export?format=csv — CSV export
    [HttpGet("export")]
    public async Task<IActionResult> ExportReport([FromQuery] string format = "csv")
    {
        var users = await _context.Users.CountAsync();
        var tutors = await _context.Users.CountAsync(x => x.Role == UserRole.Tutor);
        var students = await _context.Users.CountAsync(x => x.Role == UserRole.Student);
        var listings = await _context.TeacherListings.CountAsync();
        var pending = await _context.TeacherListings.CountAsync(x => x.Status == "PendingApproval");
        var active = await _context.TeacherListings.CountAsync(x => x.Status == "Active");
        var reviews = await _context.Reviews.CountAsync();
        var reports = await _context.Reports.CountAsync();
        var messages = await _context.Messages.CountAsync();

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Metrik,Değer");
        csv.AppendLine($"Toplam Kullanıcı,{users}");
        csv.AppendLine($"Toplam Eğitmen,{tutors}");
        csv.AppendLine($"Toplam Öğrenci,{students}");
        csv.AppendLine($"Toplam İlan,{listings}");
        csv.AppendLine($"Onay Bekleyen İlan,{pending}");
        csv.AppendLine($"Aktif İlan,{active}");
        csv.AppendLine($"Toplam Yorum,{reviews}");
        csv.AppendLine($"Toplam Şikayet,{reports}");
        csv.AppendLine($"Toplam Mesaj,{messages}");
        csv.AppendLine($"Rapor Tarihi,{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");

        var fileName = $"teacherfind-report-{DateTime.UtcNow:yyyyMMdd}.csv";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

        return File(bytes, "text/csv", fileName);
    }
}