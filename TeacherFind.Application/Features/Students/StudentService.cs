using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Students;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Application.Features.Students;

public class StudentService : IStudentService
{
    private readonly IBookingRepository _bookingRepository;

    public StudentService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<StudentDashboardStatsDto> GetDashboardStatsAsync(Guid studentUserId)
    {
        var bookings = await _bookingRepository.GetByStudentUserIdAsync(studentUserId);

        var now = DateTime.UtcNow;

        return new StudentDashboardStatsDto
        {
            UpcomingLessons = bookings.Count(x =>
                x.Status == BookingStatus.Approved &&
                x.StartTime > now),

            CompletedLessons = bookings.Count(x =>
                x.Status == BookingStatus.Completed),

            PendingBookings = bookings.Count(x =>
                x.Status == BookingStatus.Pending),

            CancelledLessons = bookings.Count(x =>
                x.Status == BookingStatus.Cancelled),

            TotalTutorsContacted = bookings
                .Select(x => x.TutorUserId)
                .Distinct()
                .Count(),

            RemainingSubscriptionHours = 0
        };
    }

    public Task GetLessonHistoryAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}