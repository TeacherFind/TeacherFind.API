using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Listings;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Reviews;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    // ⭐ YORUM EKLE
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

    // 📄 YORUMLARI GETİR
    public async Task<List<Review>> GetReviewsAsync(Guid listingId)
    {
        return await _reviewRepository.GetByListingIdAsync(listingId);
    }

    // ⭐ ORTALAMA PUAN
    public async Task<double> GetAverageRatingAsync(Guid listingId)
    {
        return await _reviewRepository.GetAverageRatingAsync(listingId);
    }

    // 🔢 YORUM SAYISI
    public async Task<int> GetReviewCountAsync(Guid listingId)
    {
        return await _reviewRepository.GetReviewCountAsync(listingId);
    }
}