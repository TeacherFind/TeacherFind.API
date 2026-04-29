using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Tutors;

public class TutorService : ITutorService
{
    private readonly IListingRepository _listingRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ITeacherRepository _teacherRepository;

    public TutorService(
        IListingRepository listingRepository,
        IFavoriteRepository favoriteRepository,
        IReviewRepository reviewRepository,
        ITeacherRepository teacherRepository)
    {
        _listingRepository = listingRepository;
        _favoriteRepository = favoriteRepository;
        _reviewRepository = reviewRepository;
        _teacherRepository = teacherRepository;
    }

    // Task 5
    public async Task<PagedResultDto<TutorListItemDto>> GetTutorsAsync(
        TutorFilterRequestDto filter,
        Guid? currentUserId)
    {
        var (items, totalCount) = await _listingRepository.FilterTutorsAsync(filter);

        HashSet<Guid> favoriteIds = new();

        if (currentUserId.HasValue)
        {
            var favorites = await _favoriteRepository.GetUserFavoritesAsync(currentUserId.Value);
            favoriteIds = favorites.Select(x => x.ListingId).ToHashSet();
        }

        var tutors = items.Select(x => new TutorListItemDto
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
            Items = tutors,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // Task 6
    public async Task<TutorDetailDto?> GetTutorByIdAsync(
        Guid listingId,
        Guid? currentUserId)
    {
        var listing = await _listingRepository.GetByIdWithFullDetailsAsync(listingId);

        if (listing is null)
            return null;

        listing.ViewCount += 1;
        await _listingRepository.SaveChangesAsync();

        var reviews = await _reviewRepository.GetByListingIdWithReviewerAsync(listingId);
        var averageRating = reviews.Count > 0
            ? reviews.Average(x => x.Rating)
            : 0;

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
            Rating = Math.Round(averageRating, 1),
            ReviewCount = reviews.Count,
            ViewCount = listing.ViewCount,
            Status = listing.Status,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            Reviews = reviews.Select(x => new TutorReviewDto
            {
                Id = x.Id,
                UserId = x.UserId,
                ReviewerName = x.Reviewer?.FullName ?? "Anonim",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt
            }).ToList(),
            Availability = profile.Availabilities.Select(x => new TutorAvailabilityDto
            {
                Day = x.Day,
                Start = x.Start,
                End = x.End
            }).ToList(),
            Documents = profile.Certificates.Select(x => new TutorCertificateDto
            {
                Name = x.Name,
                Organization = x.Organization,
                Year = x.Year
            }).ToList()
        };
    }

    public async Task<bool> UpdateMyProfileAsync(
        Guid currentUserId,
        UpdateTutorProfileDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(currentUserId);

        if (profile is null)
        {
            profile = new TeacherProfile
            {
                UserId = currentUserId,
                Rating = 0,
                TotalReviews = 0,
                IsStudent = false
            };

            await _teacherRepository.AddAsync(profile);
        }

        profile.Headline = request.Headline?.Trim();
        profile.Bio = request.Bio?.Trim();
        profile.TeachingStyle = request.TeachingStyle?.Trim();
        profile.City = request.City?.Trim();
        profile.UniversityId = request.UniversityId;
        profile.DepartmentId = request.DepartmentId;
        profile.UpdatedAt = DateTime.UtcNow;

        await _teacherRepository.SaveChangesAsync();

        return true;
    }
}