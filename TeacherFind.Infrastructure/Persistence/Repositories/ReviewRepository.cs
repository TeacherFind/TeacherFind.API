using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Review review)
    {
        await _context.Reviews.AddAsync(review);
    }

    public async Task<List<Review>> GetByListingIdAsync(Guid listingId)
    {
        return await _context.Reviews
            .Where(x => x.ListingId == listingId)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync(Guid listingId)
    {
        return await _context.Reviews
            .Where(x => x.ListingId == listingId)
            .AverageAsync(x => (double?)x.Rating) ?? 0;
    }

    public async Task<int> GetReviewCountAsync(Guid listingId)
    {
        return await _context.Reviews
            .CountAsync(x => x.ListingId == listingId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}