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

    public Dictionary<string, string> SocialLinks { get; set; } = new();
}

public class UpdateSystemSettingsRequest
{
    public string SiteTitle { get; set; } = default!;

    public string ContactEmail { get; set; } = default!;

    public bool MaintenanceMode { get; set; }

    public int CommissionRate { get; set; }

    public decimal MinWithdrawal { get; set; }

    public Dictionary<string, string> SocialLinks { get; set; } = new();
}