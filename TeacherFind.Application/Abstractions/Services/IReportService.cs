using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Reports;

namespace TeacherFind.Application.Abstractions.Services;

public interface IReportService
{
    Task CreateReportAsync(Guid reporterId, CreateReportDto dto);
    Task<List<ReportDto>> GetAllReportsAsync();
    Task<bool> ResolveReportAsync(Guid reportId, ResolveReportDto dto);
}
