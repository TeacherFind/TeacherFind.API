using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Reviews;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Application.Features.Reviews;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ITeacherRepository _teacherProfileRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IBookingRepository bookingRepository,
        ITeacherRepository teacherProfileRepository)
    {
        _reviewRepository = reviewRepository;
        _bookingRepository = bookingRepository;
        _teacherProfileRepository = teacherProfileRepository;
    }

    // Legacy — kept for backwards compatibility
    public async Task AddReviewAsync(Guid userId, Guid listingId, int rating, string comment)
    {
        var review = new Review
        {
            UserId = userId,
            ListingId = listingId,
            Rating = rating,
            Comment = comment
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();
    }

    public async Task<List<Review>> GetReviewsAsync(Guid listingId)
        => await _reviewRepository.GetByListingIdAsync(listingId);

    public async Task<double> GetAverageRatingAsync(Guid listingId)
        => await _reviewRepository.GetAverageRatingAsync(listingId);

    public async Task<int> GetReviewCountAsync(Guid listingId)
        => await _reviewRepository.GetReviewCountAsync(listingId);

    public async Task CreateReviewAsync(Guid userId, CreateReviewRequestDto dto)
    {
        // 1. Rating kontrolü
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new Exception("Puan 1 ile 5 arasında olmalıdır.");

        // 2. Booking bul
        var booking = await _bookingRepository.GetByIdWithListingAsync(dto.BookingId);
        if (booking == null)
            throw new Exception("Ders bulunamadı.");

        // 3. Sahiplik kontrolü
        if (booking.StudentUserId != userId)
            throw new Exception("Bu derse ait yorum yapma yetkiniz yok.");

        // 4. Ders tamamlandı mı?
        if (booking.Status != BookingStatus.Completed)
            throw new Exception("Sadece tamamlanmış dersler için yorum yapılabilir.");

        // 5. Daha önce yorum yapılmış mı?
        var alreadyReviewedThisListing = await _reviewRepository.ExistsByUserAndListingIdAsync(
            userId,
            booking.TeacherListingId);

        if (alreadyReviewedThisListing)
            throw new Exception("Bu ilana daha önce yorum yaptınız.");
        // 6. Review oluştur
        var teacherProfileId = booking.TeacherListing?.TeacherProfileId;

        var review = new Review
        {
            UserId = userId,
            ListingId = booking.TeacherListingId,
            TeacherProfileId = teacherProfileId,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment?.Trim() ?? ""
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();

        // 7. Öğretmen profilini güncelle
        if (teacherProfileId == null) return;

        var profile = await _teacherProfileRepository.GetByIdAsync(teacherProfileId.Value);
        if (profile == null) return;

        // Fixed: uses TeacherProfileId-based methods, not ListingId-based
        var avgRating = await _reviewRepository
            .GetAverageRatingByTeacherProfileIdAsync(teacherProfileId.Value);
        var count = await _reviewRepository
            .GetReviewCountByTeacherProfileIdAsync(teacherProfileId.Value);

        profile.Rating = Math.Round(avgRating, 1);
        profile.TotalReviews = count;

        // Fixed: calls the typed overload, not the object overload
        _teacherProfileRepository.Update(profile);
        await _teacherProfileRepository.SaveChangesAsync();
    }
}