using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync();
}
