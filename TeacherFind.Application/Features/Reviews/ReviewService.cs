using System;
using Microsoft.EntityFrameworkCore;
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

    // OLD METHOD (kept for compatibility)
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

    // ✅ CLEAN VERSION (NO DbContext)
    public async Task CreateReviewAsync(Guid userId, CreateReviewRequestDto dto)
    {
        // Rating validation
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new Exception("Puan 1 ile 5 arasında olmalıdır.");

        // Get booking
        var booking = await _bookingRepository.GetByIdWithListingAsync(dto.BookingId);

        if (booking == null)
            throw new Exception("Ders bulunamadı.");

        // Ownership check
        if (booking.StudentUserId != userId)
            throw new Exception("Bu derse ait yorum yapma yetkiniz yok.");

        // Status check
        if (booking.Status != BookingStatus.Completed)
            throw new Exception("Sadece tamamlanmış dersler için yorum yapılabilir.");

        // Prevent duplicate review
        var alreadyReviewed = await _reviewRepository.ExistsByBookingIdAsync(dto.BookingId);
        if (alreadyReviewed)
            throw new Exception("Bu ders için zaten bir yorum bıraktınız.");

        // Create review
        var review = new Review
        {
            UserId = userId,
            ListingId = booking.TeacherListingId,
            TeacherProfileId = booking.TeacherListing?.TeacherProfileId,
            BookingId = dto.BookingId,
            Rating = dto.Rating,
            Comment = dto.Comment ?? ""
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();

        // Update teacher profile
        if (review.TeacherProfileId != null)
        {
            var profile = await _teacherProfileRepository.GetByIdAsync(review.TeacherProfileId.Value);

            if (profile != null)
            {
                // FIX 1: Make sure you are getting the average for the TEACHER, not just one LISTING.
                // (Assuming your repository methods expect a TeacherProfileId)
                var avgRating = await _reviewRepository.GetAverageRatingAsync(profile.Id);
                var count = await _reviewRepository.GetReviewCountAsync(profile.Id);

                // FIX 2: Cast to decimal. Math.Round often returns a double, which fails if profile.Rating is a decimal.
                profile.Rating = (double)Math.Round(avgRating, 1);
                profile.TotalReviews = count;

                // FIX 3: In Entity Framework Core, Update is almost always synchronous.
                _teacherProfileRepository.Update(profile);
                await _teacherProfileRepository.SaveChangesAsync();
            }
        }
    }
}