using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Report report)
        => await _context.Reports.AddAsync(report);

    public async Task<List<Report>> GetAllAsync()
        => await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.TargetListing)
            .Include(r => r.TargetUser)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<Report?> GetByIdAsync(Guid id)
        => await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}