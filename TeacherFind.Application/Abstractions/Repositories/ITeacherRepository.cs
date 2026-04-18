using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface ITeacherRepository
{
    Task<TeacherProfile?> GetByUserIdAsync(Guid userId);

    Task AddAsync(TeacherProfile teacher);

    Task SaveChangesAsync();
}