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

    public async Task<TeacherProfile?> GetByUserIdWithCertificatesAsync(Guid userId)
    {
        return await _context.TeacherProfiles
            .AsNoTracking()
            .Include(x => x.Certificates)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<TeacherCertificate?> GetCertificateForUserAsync(Guid userId, Guid certificateId)
    {
        return await _context.TeacherCertificates
            .Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x =>
                x.Id == certificateId &&
                x.TeacherProfile.UserId == userId);
    }

    public async Task AddCertificateAsync(TeacherCertificate certificate)
    {
        await _context.TeacherCertificates.AddAsync(certificate);
    }

    public void RemoveCertificate(TeacherCertificate certificate)
    {
        _context.TeacherCertificates.Remove(certificate);
    }
}