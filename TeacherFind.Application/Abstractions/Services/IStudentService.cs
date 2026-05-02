using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Students;

namespace TeacherFind.Application.Abstractions.Services;

public interface IStudentService
{
    Task<StudentDashboardStatsDto> GetDashboardStatsAsync(Guid studentUserId);

    Task<List<StudentLessonHistoryDto>> GetLessonHistoryAsync(Guid studentUserId);

    Task<StudentProfileDto?> GetMyProfileAsync(Guid studentUserId);

    Task<bool> UpdateMyProfileAsync(Guid studentUserId, UpdateStudentProfileDto request);
}