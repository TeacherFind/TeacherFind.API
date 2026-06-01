using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Bookings;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Application.Features.Bookings;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IListingRepository _listingRepository;
    private readonly INotificationService _notificationService;

    public BookingService(
        IBookingRepository bookingRepository,
        IListingRepository listingRepository,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _listingRepository = listingRepository;
        _notificationService = notificationService;
    }

    public async Task<BookingDto> CreateAsync(
        Guid studentUserId,
        CreateBookingRequestDto request)
    {
        if (request.EndTime <= request.StartTime)
            throw new InvalidOperationException("Bitiş zamanı başlangıç zamanından sonra olmalıdır.");

        var listing = await _listingRepository.GetByIdWithDetailsAsync(request.TeacherListingId);

        if (listing is null)
            throw new InvalidOperationException("İlan bulunamadı.");

        if (!listing.IsActive || !listing.IsApproved)
            throw new InvalidOperationException("Bu ilan şu anda rezervasyon için uygun değil.");

        var tutorUserId = listing.TeacherProfile.UserId;

        var hasConflict = await _bookingRepository.HasTutorTimeConflictAsync(
            tutorUserId,
            request.StartTime,
            request.EndTime);

        if (hasConflict)
            throw new InvalidOperationException("Bu saat aralığı dolu. Lütfen başka bir saat seçiniz.");

        var booking = new Booking
        {
            TeacherListingId = listing.Id,
            StudentUserId = studentUserId,
            TutorUserId = tutorUserId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Source = request.Source,
            StudentNote = request.StudentNote?.Trim(),
            Status = BookingStatus.Pending
        };

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            tutorUserId,
            "Yeni ders talebi",
            $"{request.StartTime:dd.MM.yyyy HH:mm} için yeni bir ders talebi aldınız.",
            "Booking");

        var createdBooking = await _bookingRepository.GetByIdWithDetailsAsync(booking.Id);

        return MapToDto(createdBooking ?? booking);
    }

    public async Task<List<BookingDto>> GetMyBookingsAsync(Guid currentUserId)
    {
        var bookings = await _bookingRepository.GetByStudentUserIdAsync(currentUserId);

        return bookings
            .Select(MapToDto)
            .ToList();
    }

    public async Task<List<BookingDto>> GetTutorBookingsAsync(Guid tutorUserId)
    {
        var bookings = await _bookingRepository.GetByTutorUserIdAsync(tutorUserId);

        return bookings
            .Select(MapToDto)
            .ToList();
    }

    public async Task<List<OccupiedBookingSlotDto>> GetOccupiedSlotsAsync(
        Guid teacherListingId,
        DateTime from,
        DateTime to)
    {
        if (to <= from)
            throw new InvalidOperationException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        var bookings = await _bookingRepository.GetOccupiedSlotsByListingAsync(
            teacherListingId,
            from,
            to);

        return bookings
            .Select(x => new OccupiedBookingSlotDto
            {
                BookingId = x.Id,
                TeacherListingId = x.TeacherListingId,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Status = x.Status.ToString()
            })
            .ToList();
    }

    public async Task<bool> ApproveAsync(Guid bookingId, Guid tutorUserId)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null || booking.TutorUserId != tutorUserId)
            return false;

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen rezervasyonlar onaylanabilir.");

        booking.Status = BookingStatus.Approved;
        booking.ApprovedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            booking.StudentUserId,
            "Ders talebiniz onaylandı",
            $"{booking.StartTime:dd.MM.yyyy HH:mm} tarihindeki ders talebiniz onaylandı.",
            "Booking");

        return true;
    }

    public async Task<bool> RejectAsync(
        Guid bookingId,
        Guid tutorUserId,
        RejectBookingRequestDto request)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null || booking.TutorUserId != tutorUserId)
            return false;

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen rezervasyonlar reddedilebilir.");

        booking.Status = BookingStatus.Rejected;
        booking.TutorNote = request.TutorNote?.Trim();
        booking.RejectedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            booking.StudentUserId,
            "Ders talebiniz reddedildi",
            $"{booking.StartTime:dd.MM.yyyy HH:mm} tarihindeki ders talebiniz reddedildi.",
            "Booking");

        return true;
    }

    public async Task<bool> CancelAsync(
        Guid bookingId,
        Guid currentUserId,
        CancelBookingRequestDto request)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null)
            return false;

        var isStudentOwner = booking.StudentUserId == currentUserId;
        var isTutorOwner = booking.TutorUserId == currentUserId;

        if (!isStudentOwner && !isTutorOwner)
            return false;

        if (booking.Status == BookingStatus.Completed)
            throw new InvalidOperationException("Tamamlanmış ders iptal edilemez.");

        booking.Status = BookingStatus.Cancelled;
        booking.TutorNote = request.Reason?.Trim();
        booking.CancelledAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync();

        var targetUserId = isStudentOwner
            ? booking.TutorUserId
            : booking.StudentUserId;

        await _notificationService.SendNotificationAsync(
            targetUserId,
            "Ders iptal edildi",
            $"{booking.StartTime:dd.MM.yyyy HH:mm} tarihindeki ders iptal edildi.",
            "Booking");

        return true;
    }

    public async Task<bool> CompleteAsync(Guid bookingId, Guid tutorUserId)
    {
        var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);

        if (booking is null || booking.TutorUserId != tutorUserId)
            return false;

        if (booking.Status != BookingStatus.Approved)
            throw new InvalidOperationException("Sadece onaylanmış ders tamamlandı yapılabilir.");

        booking.Status = BookingStatus.Completed;
        booking.CompletedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        await _bookingRepository.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            booking.StudentUserId,
            "Ders tamamlandı",
            $"{booking.StartTime:dd.MM.yyyy HH:mm} tarihindeki ders tamamlandı olarak işaretlendi.",
            "Booking");

        return true;
    }

    private static BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            TeacherListingId = booking.TeacherListingId,
            LessonTitle = booking.TeacherListing?.Title ?? string.Empty,
            StudentUserId = booking.StudentUserId,
            StudentName = booking.StudentUser?.FullName ?? string.Empty,
            TutorUserId = booking.TutorUserId,
            TutorName = booking.TutorUser?.FullName ?? string.Empty,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status.ToString(),
            Source = booking.Source.ToString(),
            StudentNote = booking.StudentNote,
            TutorNote = booking.TutorNote,
            CreatedAt = booking.CreatedAt
        };
    }
}