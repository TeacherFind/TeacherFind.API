using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Reports;

public class ResolveReportDto
{
    /// <summary>Resolved | Dismissed</summary>
    public string Status { get; set; } = default!;
    public string? AdminNote { get; set; }
}