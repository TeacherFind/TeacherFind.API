using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Review review)
        => await _context.Reviews.AddAsync(review);

    public async Task<List<Review>> GetByListingIdAsync(Guid listingId)
        => await _context.Reviews
            .Where(x => x.ListingId == listingId)
            .ToListAsync();

    public async Task<List<Review>> GetByListingIdWithReviewerAsync(Guid listingId)
        => await _context.Reviews
            .Include(x => x.Reviewer)
            .Where(x => x.ListingId == listingId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<bool> ExistsByBookingIdAsync(Guid bookingId)
        => await _context.Reviews.AnyAsync(x => x.BookingId == bookingId);

    public async Task<double> GetAverageRatingAsync(Guid listingId)
        => await _context.Reviews
            .Where(x => x.ListingId == listingId)
            .AverageAsync(x => (double?)x.Rating) ?? 0;

    public async Task<int> GetReviewCountAsync(Guid listingId)
        => await _context.Reviews.CountAsync(x => x.ListingId == listingId);

    public async Task<double> GetAverageRatingByTeacherProfileIdAsync(Guid teacherProfileId)
        => await _context.Reviews
            .Where(x => x.TeacherProfileId == teacherProfileId)
            .AverageAsync(x => (double?)x.Rating) ?? 0;

    public async Task<int> GetReviewCountByTeacherProfileIdAsync(Guid teacherProfileId)
        => await _context.Reviews
            .CountAsync(x => x.TeacherProfileId == teacherProfileId);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}