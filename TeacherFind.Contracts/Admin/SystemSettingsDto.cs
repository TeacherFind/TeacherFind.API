using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class SystemSettingsDto
{
    public string SiteTitle { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public bool MaintenanceMode { get; set; }
    public int CommissionRate { get; set; }
    public decimal MinWithdrawal { get; set; }
    public string? SiteDescription { get; set; }
    public string? SiteKeywords { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public Dictionary<string, string> SocialLinks { get; set; } = new();
}

public class UpdateSystemSettingsRequest
{
    public string SiteTitle { get; set; } = default!;
    public string ContactEmail { get; set; } = default!;
    public bool MaintenanceMode { get; set; }
    public int CommissionRate { get; set; }
    public decimal MinWithdrawal { get; set; }
    public string? SiteDescription { get; set; }
    public string? SiteKeywords { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? FacebookLink { get; set; }
    public string? InstagramLink { get; set; }
    public string? TwitterLink { get; set; }
    public string? LinkedInLink { get; set; }
    public string? YoutubeLink { get; set; }
    public Dictionary<string, string> SocialLinks { get; set; } = new();
}