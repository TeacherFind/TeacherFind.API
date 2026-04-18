using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Services;

public interface IReviewService
{
    Task AddReviewAsync(Guid userId, Guid listingId, int rating, string comment);

    Task<List<Review>> GetReviewsAsync(Guid listingId);

    Task<double> GetAverageRatingAsync(Guid listingId);

    Task<int> GetReviewCountAsync(Guid listingId);
}