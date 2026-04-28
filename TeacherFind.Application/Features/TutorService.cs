using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Tutors;

namespace TeacherFind.Application.Features.Tutors;

public class TutorService : ITutorService
{
    private readonly IListingRepository _listingRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IReviewRepository _reviewRepository;

    public TutorService(
        IListingRepository listingRepository,
        IFavoriteRepository favoriteRepository,
        IReviewRepository reviewRepository)
    {
        _listingRepository = listingRepository;
        _favoriteRepository = favoriteRepository;
        _reviewRepository = reviewRepository;
    }

    // Task 5
    public async Task<PagedResultDto<TutorListItemDto>> GetTutorsAsync(
        TutorFilterRequestDto filter, Guid? currentUserId)
    {
        var (items, totalCount) = await _listingRepository.FilterTutorsAsync(filter);

        // Load all favorites in one query — no N+1
        HashSet<Guid> favoriteIds = new();
        if (currentUserId.HasValue)
        {
            var favs = await _favoriteRepository.GetUserFavoritesAsync(currentUserId.Value);
            favoriteIds = favs.Select(f => f.ListingId).ToHashSet();
        }

        var dtos = items.Select(x => new TutorListItemDto
        {
            Id = x.Id,
            TeacherProfileId = x.TeacherProfileId,
            TeacherName = x.TeacherProfile.User.FullName,
            Title = x.Title,
            Description = x.Description,
            Price = x.Price,
            ServiceType = x.ServiceType.ToString(),
            City = x.City?.Name,
            District = x.District?.Name,
            Neighborhood = x.Neighborhood?.Name,
            Subject = x.Subject?.Name,
            Rating = x.TeacherProfile.Rating,
            ReviewCount = x.TeacherProfile.TotalReviews,
            IsFavorite = favoriteIds.Contains(x.Id)
        }).ToList();

        return new PagedResultDto<TutorListItemDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // Task 6
    public async Task<TutorDetailDto?> GetTutorByIdAsync(Guid listingId, Guid? currentUserId)
    {
        var listing = await _listingRepository.GetByIdWithFullDetailsAsync(listingId);
        if (listing is null) return null;

        listing.ViewCount += 1;
        await _listingRepository.SaveChangesAsync();

        var reviews = await _reviewRepository.GetByListingIdWithReviewerAsync(listingId);
        var avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;

        var profile = listing.TeacherProfile;

        return new TutorDetailDto
        {
            Id = listing.Id,
            TeacherProfileId = listing.TeacherProfileId,
            TeacherName = profile.User.FullName,
            AvatarUrl = profile.User.ProfileImageUrl,
            Title = listing.Title,
            Headline = profile.Headline,
            Bio = profile.Bio,
            TeachingStyle = profile.TeachingStyle,
            Price = listing.Price,
            LessonDuration = listing.LessonDuration,
            ServiceType = listing.ServiceType.ToString(),
            Subject = listing.Subject?.Name,
            Category = listing.Category,
            City = listing.City?.Name,
            District = listing.District?.Name,
            Neighborhood = listing.Neighborhood?.Name,
            University = profile.University?.Name,
            Department = profile.DepartmentEntity?.Name,
            Rating = Math.Round(avgRating, 1),
            ReviewCount = reviews.Count,
            ViewCount = listing.ViewCount,
            Status = listing.Status,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            Reviews = reviews.Select(r => new TutorReviewDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ReviewerName = r.Reviewer?.FullName ?? "Anonim",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList(),
            Availability = profile.Availabilities.Select(a => new TutorAvailabilityDto
            {
                Day = a.Day,
                Start = a.Start,
                End = a.End
            }).ToList(),
            Documents = profile.Certificates.Select(c => new TutorCertificateDto
            {
                Name = c.Name,
                Organization = c.Organization,
                Year = c.Year
            }).ToList()
        };
    }
}