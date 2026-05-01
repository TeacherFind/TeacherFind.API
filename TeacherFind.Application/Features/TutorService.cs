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
    private readonly IBookingRepository _bookingRepository;

    public TutorService(
    IListingRepository listingRepository,
    IFavoriteRepository favoriteRepository,
    IReviewRepository reviewRepository,
    ITeacherRepository teacherRepository,
    IBookingRepository bookingRepository)
    {
        _listingRepository = listingRepository;
        _favoriteRepository = favoriteRepository;
        _reviewRepository = reviewRepository;
        _teacherRepository = teacherRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResultDto<TutorListItemDto>> GetTutorsAsync(
        TutorFilterRequestDto filter,
        Guid? currentUserId)
    {
        var (items, totalCount) = await _listingRepository.FilterTutorsAsync(filter);

        var favoriteIds = new HashSet<Guid>();

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

        return listings.Select(MapToMyTutorListingDto).ToList();
    }

    public async Task<MyTutorListingDto> CreateMyListingAsync(
        Guid currentUserId,
        CreateMyTutorListingDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(currentUserId);

        if (profile is null)
            throw new InvalidOperationException("Öğretmen profili bulunamadı.");

        var branchAlreadyExists = await _listingRepository.ExistsForTeacherBranchAsync(
            currentUserId,
            request.SubjectId,
            request.Category,
            request.SubCategory);

        if (branchAlreadyExists)
            throw new InvalidOperationException("Bu branş için zaten bir ilanınız var.");

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

        return MapToMyTutorListingDto(listing);
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

        var branchAlreadyExists = await _listingRepository.ExistsForTeacherBranchAsync(
            currentUserId,
            request.SubjectId,
            request.Category,
            request.SubCategory,
            listing.Id);

        if (branchAlreadyExists)
            throw new InvalidOperationException("Bu branş için zaten başka bir ilanınız var.");

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

        // İlan değişince tekrar admin onayına düşer.
        listing.IsApproved = false;
        listing.Status = "Pending";

        await _listingRepository.SaveChangesAsync();

        return MapToMyTutorListingDto(listing);
    }

    private static MyTutorListingDto MapToMyTutorListingDto(TeacherListing listing)
    {
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

    public async Task<List<MyStudentDto>> GetMyStudentsAsync(Guid currentUserId)
    {
        var completedBookings = await _bookingRepository.GetCompletedByTutorUserIdAsync(currentUserId);

        return completedBookings
            .GroupBy(x => x.StudentUserId)
            .Select(group =>
            {
                var lastBooking = group
                    .OrderByDescending(x => x.StartTime)
                    .First();

                return new MyStudentDto
                {
                    StudentId = lastBooking.StudentUserId,
                    StudentName = lastBooking.StudentUser.FullName,
                    TeacherListingId = lastBooking.TeacherListingId,
                    LessonTitle = lastBooking.TeacherListing.Title,
                    LastLessonDate = lastBooking.StartTime,
                    Source = lastBooking.Source.ToString(),
                    CompletedLessonCount = group.Count()
                };
            })
            .OrderByDescending(x => x.LastLessonDate)
            .ToList();
    }

    public async Task<TutorProfileDto?> GetMyProfileAsync(Guid userId)
    {
        var profile = await _listingRepository.GetTeacherProfileByUserIdAsync(userId);

        if (profile is null)
            return null;

        return new TutorProfileDto
        {
            UserId = profile.UserId,
            TeacherProfileId = profile.Id,

            FullName = profile.User.FullName,
            Email = profile.User.Email,
            PhoneNumber = profile.User.PhoneNumber,
            ProfileImageUrl = profile.User.ProfileImageUrl,

            Title = profile.Title,
            Headline = profile.Headline,
            Bio = profile.Bio,
            City = profile.City,

            UniversityId = profile.UniversityId,
            UniversityName = profile.University?.Name,

            DepartmentId = profile.DepartmentId,
            DepartmentName = profile.DepartmentEntity?.Name,

            EducationLevel = profile.EducationLevel,
            IsStudent = profile.IsStudent,
            TeachingStyle = profile.TeachingStyle,

            Rating = profile.Rating,
            TotalReviews = profile.TotalReviews,

            Certificates = profile.Certificates.Select(c => new TutorProfileCertificateDto
            {
                Id = c.Id,
                Name = c.Name,
                Organization = c.Organization,
                Year = c.Year,
                FileUrl = c.FileUrl,
                FileName = c.FileName,
                ContentType = c.ContentType
            }).ToList(),

            Availabilities = profile.Availabilities.Select(a => new TutorProfileAvailabilityDto
            {
                Id = a.Id,
                Day = a.Day,
                Start = a.Start,
                End = a.End
            }).ToList()
        };
    }

    public async Task<List<TutorProfileCertificateDto>> GetMyCertificatesAsync(Guid currentUserId)
    {
        var profile = await _teacherRepository.GetByUserIdWithCertificatesAsync(currentUserId);

        if (profile is null)
            return new List<TutorProfileCertificateDto>();

        return profile.Certificates
            .OrderByDescending(x => x.Year)
            .Select(x => new TutorProfileCertificateDto
            {
                Id = x.Id,
                Name = x.Name,
                Organization = x.Organization,
                Year = x.Year,
                FileUrl = x.FileUrl,
                FileName = x.FileName,
                ContentType = x.ContentType
            })
            .ToList();
    }

    public async Task<TutorProfileCertificateDto> AddMyCertificateAsync(
        Guid currentUserId,
        AddTutorCertificateDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(currentUserId);

        if (profile is null)
            throw new InvalidOperationException("Öğretmen profili bulunamadı.");

        var certificate = new TeacherCertificate
        {
            TeacherProfileId = profile.Id,
            Name = request.Name.Trim(),
            Organization = request.Organization.Trim(),
            Year = request.Year,
            FileUrl = request.FileUrl,
            FileName = request.FileName,
            ContentType = request.ContentType
        };

        await _teacherRepository.AddCertificateAsync(certificate);
        await _teacherRepository.SaveChangesAsync();

        return new TutorProfileCertificateDto
        {
            Id = certificate.Id,
            Name = certificate.Name,
            Organization = certificate.Organization,
            Year = certificate.Year,
            FileUrl = certificate.FileUrl,
            FileName = certificate.FileName,
            ContentType = certificate.ContentType
        };
    }

    public async Task<bool> DeleteMyCertificateAsync(Guid currentUserId, Guid certificateId)
    {
        var certificate = await _teacherRepository.GetCertificateForUserAsync(
            currentUserId,
            certificateId);

        if (certificate is null)
            return false;

        _teacherRepository.RemoveCertificate(certificate);
        await _teacherRepository.SaveChangesAsync();

        return true;
    }
}