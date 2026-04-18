using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Listings;
using TeacherFind.Domain.Entities;


namespace TeacherFind.Application.Features.Favorites;

public class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IListingRepository _listingRepository;

    public FavoriteService(IFavoriteRepository favoriteRepository,
                           IListingRepository listingRepository)
    {
        _favoriteRepository = favoriteRepository;
        _listingRepository = listingRepository;
    }

    // ❤️ EKLE / ❌ ÇIKAR (TOGGLE)
    public async Task ToggleFavoriteAsync(Guid userId, Guid listingId)
    {
        var existing = await _favoriteRepository.GetAsync(userId, listingId);

        if (existing != null)
        {
            // ❌ Favoriden çıkar
            await _favoriteRepository.RemoveAsync(existing);
        }
        else
        {
            // ❤️ Favoriye ekle
            var favorite = new Favorite
            {
                UserId = userId,
                ListingId = listingId
            };

            await _favoriteRepository.AddAsync(favorite);
        }

        await _favoriteRepository.SaveChangesAsync();
    }

    // 📄 FAVORİ LİSTESİ
    public async Task<List<ListingDto>> GetFavoritesAsync(Guid userId)
    {
        var favorites = await _favoriteRepository.GetUserFavoritesAsync(userId);

        var result = new List<ListingDto>();

        foreach (var fav in favorites)
        {
            var listing = await _listingRepository.GetByIdAsync(fav.ListingId);

            if (listing != null)
            {
                result.Add(new ListingDto
                {
                    Id = listing.Id,
                    Title = listing.Title,
                    Description = listing.Description,
                    Price = listing.Price,
                    ViewCount = listing.ViewCount
                });
            }
        }

        return result;
    }
}