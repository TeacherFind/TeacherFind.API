using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.TeacherProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(TeacherProfile teacher)
    {
        await _context.TeacherProfiles.AddAsync(teacher);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}