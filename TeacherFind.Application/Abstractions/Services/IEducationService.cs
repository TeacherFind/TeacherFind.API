using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Education;

namespace TeacherFind.Application.Abstractions.Services;

public interface IEducationService
{
    Task<List<UniversityDto>> GetUniversitiesAsync();

    Task<List<DepartmentDto>> GetDepartmentsAsync(Guid? universityId);
}
