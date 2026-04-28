using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Listings;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Favorites;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository)
        => _favoriteRepository = favoriteRepository;

    /// <returns>true = added, false = removed</returns>
    public async Task<bool> ToggleFavoriteAsync(Guid userId, Guid listingId)
    {
        var existing = await _favoriteRepository.GetAsync(userId, listingId);

        if (existing != null)
        {
            await _favoriteRepository.RemoveAsync(existing);
            await _favoriteRepository.SaveChangesAsync();
            return false;
        }

        await _favoriteRepository.AddAsync(new Favorite { UserId = userId, ListingId = listingId });
        await _favoriteRepository.SaveChangesAsync();
        return true;
    }

    public async Task<List<ListingDto>> GetFavoritesAsync(Guid userId)
    {
        // Single JOIN query — N+1 fixed
        var favorites = await _favoriteRepository.GetUserFavoritesWithListingsAsync(userId);

        return favorites
            .Where(f => f.Listing is { IsActive: true })
            .Select(f => new ListingDto
            {
                Id = f.Listing!.Id,
                TeacherProfileId = f.Listing.TeacherProfileId,
                Title = f.Listing.Title,
                Description = f.Listing.Description,
                Price = f.Listing.Price,
                IsActive = f.Listing.IsActive,
                IsApproved = f.Listing.IsApproved,
                ViewCount = f.Listing.ViewCount
            })
            .ToList();
    }
}