using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    public BookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Bookings
            .Include(x => x.TeacherListing)
            .Include(x => x.StudentUser)
            .Include(x => x.TutorUser)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Booking>> GetByStudentUserIdAsync(Guid studentUserId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(x => x.TeacherListing)
            .Include(x => x.StudentUser)
            .Include(x => x.TutorUser)
            .Where(x => x.StudentUserId == studentUserId)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByTutorUserIdAsync(Guid tutorUserId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(x => x.TeacherListing)
            .Include(x => x.StudentUser)
            .Include(x => x.TutorUser)
            .Where(x => x.TutorUserId == tutorUserId)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Booking>> GetCompletedByTutorUserIdAsync(Guid tutorUserId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(x => x.TeacherListing)
            .Include(x => x.StudentUser)
            .Include(x => x.TutorUser)
            .Where(x =>
                x.TutorUserId == tutorUserId &&
                x.Status == BookingStatus.Completed)
            .OrderByDescending(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasTutorTimeConflictAsync(
    Guid tutorUserId,
    DateTime startTime,
    DateTime endTime)
    {
        return await _context.Bookings.AnyAsync(x =>
            x.TutorUserId == tutorUserId &&
            (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Approved) &&
            startTime < x.EndTime &&
            endTime > x.StartTime);
    }

    public async Task<List<Booking>> GetOccupiedSlotsByListingAsync(
        Guid teacherListingId,
        DateTime from,
        DateTime to)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(x =>
                x.TeacherListingId == teacherListingId &&
                (x.Status == BookingStatus.Pending || x.Status == BookingStatus.Approved) &&
                x.StartTime < to &&
                x.EndTime > from)
            .OrderBy(x => x.StartTime)
            .ToListAsync();
    }
}