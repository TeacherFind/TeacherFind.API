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

    public async Task<List<MyTutorListingDto>> GetMyListingsAsync(Guid currentUserId)
    {
        var listings = await _listingRepository.GetByTeacherUserIdAsync(currentUserId);

        return listings.Select(x => new MyTutorListingDto
        {
            Id = x.Id,
            TeacherProfileId = x.TeacherProfileId,

            SubjectId = x.SubjectId,
            SubjectName = x.Subject?.Name,

            CityId = x.CityId,
            CityName = x.City?.Name,

            DistrictId = x.DistrictId,
            DistrictName = x.District?.Name,

            NeighborhoodId = x.NeighborhoodId,
            NeighborhoodName = x.Neighborhood?.Name,

            Headline = x.Headline,
            Title = x.Title,
            Description = x.Description,
            Category = x.Category,
            SubCategory = x.SubCategory,
            ServiceType = x.ServiceType.ToString(),
            LessonDuration = x.LessonDuration,
            Price = x.Price,
            Status = x.Status,
            IsActive = x.IsActive,
            IsApproved = x.IsApproved,
            ViewCount = x.ViewCount,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }

    public async Task<MyTutorListingDto> CreateMyListingAsync(
    Guid currentUserId,
    CreateMyTutorListingDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(currentUserId);

        if (profile is null)
            throw new InvalidOperationException("Öğretmen profili bulunamadı.");

        var listing = new TeacherListing
        {
            TeacherProfileId = profile.Id,
            SubjectId = request.SubjectId,
            CityId = request.CityId,
            DistrictId = request.DistrictId,
            NeighborhoodId = request.NeighborhoodId,
            Headline = request.Headline?.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            SubCategory = request.SubCategory.Trim(),
            ServiceType = request.ServiceType,
            LessonDuration = request.LessonDuration,
            Price = request.Price,
            IsActive = true,
            IsApproved = false,
            Status = "Pending",
            ViewCount = 0
        };

        await _listingRepository.AddAsync(listing);
        await _listingRepository.SaveChangesAsync();

        return new MyTutorListingDto
        {
            Id = listing.Id,
            TeacherProfileId = listing.TeacherProfileId,

            SubjectId = listing.SubjectId,
            CityId = listing.CityId,
            DistrictId = listing.DistrictId,
            NeighborhoodId = listing.NeighborhoodId,

            Headline = listing.Headline,
            Title = listing.Title,
            Description = listing.Description,
            Category = listing.Category,
            SubCategory = listing.SubCategory,
            ServiceType = listing.ServiceType.ToString(),
            LessonDuration = listing.LessonDuration,
            Price = listing.Price,
            Status = listing.Status,
            IsActive = listing.IsActive,
            IsApproved = listing.IsApproved,
            ViewCount = listing.ViewCount,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt
        };
    }

    public async Task<MyTutorListingDto?> UpdateMyListingAsync(
    Guid currentUserId,
    Guid listingId,
    UpdateMyTutorListingDto request)
    {
        var listing = await _listingRepository.GetByIdForTeacherUserAsync(
            listingId,
            currentUserId);

        if (listing is null)
            return null;

        listing.SubjectId = request.SubjectId;
        listing.CityId = request.CityId;
        listing.DistrictId = request.DistrictId;
        listing.NeighborhoodId = request.NeighborhoodId;
        listing.Headline = request.Headline?.Trim();
        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Category = request.Category.Trim();
        listing.SubCategory = request.SubCategory.Trim();
        listing.ServiceType = request.ServiceType;
        listing.LessonDuration = request.LessonDuration;
        listing.Price = request.Price;
        listing.IsActive = request.IsActive;
        listing.UpdatedAt = DateTime.UtcNow;

        // İlan değişince tekrar admin onayına düşsün.
        listing.IsApproved = false;
        listing.Status = "Pending";

        await _listingRepository.SaveChangesAsync();

        return new MyTutorListingDto
        {
            Id = listing.Id,
            TeacherProfileId = listing.TeacherProfileId,

            SubjectId = listing.SubjectId,
            SubjectName = listing.Subject?.Name,

            CityId = listing.CityId,
            CityName = listing.City?.Name,

            DistrictId = listing.DistrictId,
            DistrictName = listing.District?.Name,

            NeighborhoodId = listing.NeighborhoodId,
            NeighborhoodName = listing.Neighborhood?.Name,

            Headline = listing.Headline,
            Title = listing.Title,
            Description = listing.Description,
            Category = listing.Category,
            SubCategory = listing.SubCategory,
            ServiceType = listing.ServiceType.ToString(),
            LessonDuration = listing.LessonDuration,
            Price = listing.Price,
            Status = listing.Status,
            IsActive = listing.IsActive,
            IsApproved = listing.IsApproved,
            ViewCount = listing.ViewCount,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt
        };
    }
}