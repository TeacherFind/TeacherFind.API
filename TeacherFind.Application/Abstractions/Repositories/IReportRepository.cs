using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IReportRepository
{
    Task AddAsync(Report report);
    Task<List<Report>> GetAllAsync();
    Task<Report?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
