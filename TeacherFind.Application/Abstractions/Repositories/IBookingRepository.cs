using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IBookingRepository
{
    Task AddAsync(Booking booking);

    Task<Booking?> GetByIdAsync(Guid id);

    Task<Booking?> GetByIdWithDetailsAsync(Guid id);

    Task<List<Booking>> GetByStudentUserIdAsync(Guid studentUserId);

    Task<List<Booking>> GetByTutorUserIdAsync(Guid tutorUserId);

    Task<List<Booking>> GetCompletedByTutorUserIdAsync(Guid tutorUserId);

    Task SaveChangesAsync();

    Task<bool> HasTutorTimeConflictAsync(
    Guid tutorUserId,
    DateTime startTime,
    DateTime endTime);

    Task<List<Booking>> GetOccupiedSlotsByListingAsync(
        Guid teacherListingId,
        DateTime from,
        DateTime to);
}