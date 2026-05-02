using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Students;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Application.Features.Students;

public class StudentService : IStudentService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;

    public StudentService(
        IBookingRepository bookingRepository,
        IReviewRepository reviewRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _reviewRepository = reviewRepository;
        _userRepository = userRepository;
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

    public async Task<List<StudentLessonHistoryDto>> GetLessonHistoryAsync(Guid studentUserId)
    {
        var bookings = await _bookingRepository.GetByStudentUserIdAsync(studentUserId);

        var completedBookings = bookings
            .Where(x => x.Status == BookingStatus.Completed)
            .OrderByDescending(x => x.StartTime)
            .ToList();

        var lessonHistory = new List<StudentLessonHistoryDto>();

        foreach (var booking in completedBookings)
        {
            var hasReview = await _reviewRepository.ExistsByBookingIdAsync(booking.Id);

            lessonHistory.Add(new StudentLessonHistoryDto
            {
                BookingId = booking.Id,
                TeacherListingId = booking.TeacherListingId,
                LessonTitle = booking.TeacherListing?.Title ?? "Ders",
                TutorUserId = booking.TutorUserId,
                TutorName = booking.TutorUser?.FullName ?? "Öğretmen",
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status.ToString(),
                HasReview = hasReview
            });
        }

        return lessonHistory;
    }

    public async Task<StudentProfileDto?> GetMyProfileAsync(Guid studentUserId)
    {
        var user = await _userRepository.GetByIdAsync(studentUserId);

        if (user is null)
            return null;

        return new StudentProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            CityId = user.CityId,
            CityName = user.City?.Name,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    public async Task<bool> UpdateMyProfileAsync(
        Guid studentUserId,
        UpdateStudentProfileDto request)
    {
        var user = await _userRepository.GetByIdAsync(studentUserId);

        if (user is null)
            return false;

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();

        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        user.CityId = request.CityId;

        user.Bio = string.IsNullOrWhiteSpace(request.Bio)
            ? null
            : request.Bio.Trim();

        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAvatarAsync(
        Guid studentUserId,
        string profileImageUrl)
    {
        var user = await _userRepository.GetByIdAsync(studentUserId);

        if (user is null)
            return false;

        user.ProfileImageUrl = profileImageUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        return true;
    }
}