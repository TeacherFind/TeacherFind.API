using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Reports;

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = default!;
    public Guid? TargetListingId { get; set; }
    public string? TargetListingTitle { get; set; }
    public Guid? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public string Reason { get; set; } = default!;
    public string? Description { get; set; }
    public string Status { get; set; } = default!;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}