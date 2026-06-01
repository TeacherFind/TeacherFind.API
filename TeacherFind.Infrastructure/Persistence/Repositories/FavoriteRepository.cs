#nullable disable
using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly AppDbContext _context;

    public FavoriteRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(Favorite favorite) => await _context.Favorites.AddAsync(favorite);
    public Task RemoveAsync(Favorite favorite)
    {
        _context.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }
    public async Task<Favorite> GetAsync(Guid userId, Guid listingId)
        => await _context.Favorites
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ListingId == listingId);

    public async Task<List<Favorite>> GetUserFavoritesAsync(Guid userId)
        => await _context.Favorites.Where(x => x.UserId == userId).ToListAsync();

    // NEW — single JOIN, no N+1
    public async Task<List<Favorite>> GetUserFavoritesWithListingsAsync(Guid userId)
        => await _context.Favorites
            .Include(f => f.Listing)
                .ThenInclude(l => l.TeacherProfile)
                    .ThenInclude(p => p.User)
            .Include(f => f.Listing)
                .ThenInclude(l => l.City)
            .Include(f => f.Listing)
                .ThenInclude(l => l.Subject)
            .Where(x => x.UserId == userId)
            .ToListAsync();

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}