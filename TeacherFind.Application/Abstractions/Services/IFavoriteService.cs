using TeacherFind.Contracts.Listings;

namespace TeacherFind.Application.Abstractions.Services;

public interface IFavoriteService
{
    Task ToggleFavoriteAsync(Guid userId, Guid listingId);
    Task<List<ListingDto>> GetFavoritesAsync(Guid userId);
}