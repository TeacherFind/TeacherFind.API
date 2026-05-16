using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities;

public class SystemSetting : AuditableEntity
{
    public string SiteTitle { get; set; } = "Öğrenmenin Çilingirleri";

    public string ContactEmail { get; set; } = "info@teacherfind.com";

    public bool MaintenanceMode { get; set; } = false;

    public int CommissionRate { get; set; } = 15;

    public decimal MinWithdrawal { get; set; } = 500;

    public string SocialLinksJson { get; set; } = "{}";
    public string? SiteDescription { get; set; }
    public string? SiteKeywords { get; set; }
    public string? GoogleAnalyticsId { get; set; }
}