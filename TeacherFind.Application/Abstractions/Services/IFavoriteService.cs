using TeacherFind.Contracts.Listings;

namespace TeacherFind.Application.Abstractions.Services;

public interface IFavoriteService
{
    /// <returns>true = added, false = removed</returns>
    Task<bool> ToggleFavoriteAsync(Guid userId, Guid listingId);
    Task<List<ListingDto>> GetFavoritesAsync(Guid userId);
}