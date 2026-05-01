using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Education;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Services.Education;
public class EducationService : IEducationService
{
    private readonly AppDbContext _context;

    public EducationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UniversityDto>> GetUniversitiesAsync()
    {
        return await _context.Universities
            .AsNoTracking()
            .Where(university => university.IsActive)
            .OrderBy(university => university.Name)
            .Select(university => new UniversityDto
            {
                Id = university.Id,
                Name = university.Name
            })
            .ToListAsync();
    }

    public async Task<List<DepartmentDto>> GetDepartmentsAsync(Guid? universityId)
    {
        var departmentsQuery = _context.Departments
            .AsNoTracking()
            .Where(department => department.IsActive);

        if (universityId.HasValue)
        {
            departmentsQuery = departmentsQuery
                .Where(department => department.UniversityId == universityId.Value);
        }

        return await departmentsQuery
            .OrderBy(department => department.Name)
            .Select(department => new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                UniversityId = department.UniversityId
            })
            .ToListAsync();
    }
}