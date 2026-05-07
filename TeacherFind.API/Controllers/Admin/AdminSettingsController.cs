using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Entities;
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
        return new SystemSettingsDto
        {
            SiteTitle = settings.SiteTitle,
            ContactEmail = settings.ContactEmail,
            MaintenanceMode = settings.MaintenanceMode,
            CommissionRate = settings.CommissionRate,
            MinWithdrawal = settings.MinWithdrawal,
            SocialLinks = DeserializeSocialLinks(settings.SocialLinksJson)
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
}