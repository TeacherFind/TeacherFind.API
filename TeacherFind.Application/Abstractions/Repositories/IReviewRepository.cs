using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IReviewRepository
{
    Task AddAsync(Review review);
    Task<List<Review>> GetByListingIdAsync(Guid listingId);
    Task<List<Review>> GetByListingIdWithReviewerAsync(Guid listingId);   // NEW
    Task<double> GetAverageRatingAsync(Guid listingId);
    Task<int> GetReviewCountAsync(Guid listingId);
    Task SaveChangesAsync();
}