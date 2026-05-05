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

    // =====================================================
    // Public Tutor Listing Endpoints
    // =====================================================

    public async Task<PagedResultDto<TutorListItemDto>> GetTutorsAsync(
        TutorFilterRequestDto filter, Guid? currentUserId)
    {
        var (items, totalCount) = await _listingRepository.FilterTutorsAsync(filter);

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

    public async Task<TutorDetailDto?> GetTutorByIdAsync(Guid listingId, Guid? currentUserId)
    {
        var listing = await _listingRepository.GetByIdWithFullDetailsAsync(listingId);
        if (listing is null) return null;

        listing.ViewCount += 1;
        await _listingRepository.SaveChangesAsync();

        var reviews = await _reviewRepository.GetByListingIdWithReviewerAsync(listingId);
        var avgRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0;
        var reviewCount = reviews.Count;

        var isFavorite = false;
        if (currentUserId.HasValue)
        {
            var fav = await _favoriteRepository.GetAsync(currentUserId.Value, listingId);
            isFavorite = fav != null;
        }

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
            ReviewCount = reviewCount,
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

    // =====================================================
    // Tutor Profile
    // =====================================================

    public async Task<TutorProfileDto?> GetMyProfileAsync(Guid userId)
    {
        var profile = await _teacherRepository.GetByUserIdWithFullDetailsAsync(userId);
        if (profile is null) return null;

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
            TeachingStyle = profile.TeachingStyle,
            UniversityId = profile.UniversityId,
            UniversityName = profile.University?.Name,
            DepartmentId = profile.DepartmentId,
            DepartmentName = profile.DepartmentEntity?.Name,
            EducationLevel = profile.EducationLevel,
            IsStudent = profile.IsStudent,
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
            }).ToList(),

            Subjects = profile.Subjects.Select(s => new TutorProfileSubjectDto
            {
                Id = s.Id,
                SubjectId = s.SubjectId,
                Stage = s.Stage ?? s.Subject?.Stage,
                Category = s.Category ?? s.Subject?.Category,
                Name = s.Name ?? s.Subject?.Name,
                Level = s.Level ?? s.Subject?.Level
            }).ToList()
        };
    }

    public async Task<bool> UpdateMyProfileAsync(Guid userId, UpdateTutorProfileDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(userId);
        if (profile is null) return false;

        if (request.Headline != null) profile.Headline = request.Headline;
        if (request.Bio != null) profile.Bio = request.Bio;
        if (request.TeachingStyle != null) profile.TeachingStyle = request.TeachingStyle;
        if (request.City != null) profile.City = request.City;
        if (request.UniversityId != null) profile.UniversityId = request.UniversityId;
        if (request.DepartmentId != null) profile.DepartmentId = request.DepartmentId;

        _teacherRepository.Update(profile);
        await _teacherRepository.SaveChangesAsync();
        return true;
    }

    // =====================================================
    // Tutor Listings
    // =====================================================

    public async Task<List<MyTutorListingDto>> GetMyListingsAsync(Guid userId)
    {
        var listings = await _listingRepository.GetByTeacherUserIdAsync(userId);

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

    public async Task<MyTutorListingDto> CreateMyListingAsync(Guid userId, CreateMyTutorListingDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Öğretmen profili bulunamadı.");

        var listing = new TeacherListing
        {
            TeacherProfileId = profile.Id,
            SubjectId = request.SubjectId,
            CityId = request.CityId,
            DistrictId = request.DistrictId,
            NeighborhoodId = request.NeighborhoodId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category.Trim(),
            SubCategory = request.SubCategory.Trim(),
            ServiceType = request.ServiceType,
            LessonDuration = request.LessonDuration,
            Price = request.Price,
            Status = "Pending",
            IsActive = true,
            IsApproved = false,
            ViewCount = 0
        };

        await _listingRepository.AddAsync(listing);
        await _listingRepository.SaveChangesAsync();

        return new MyTutorListingDto
        {
            Id = listing.Id,
            TeacherProfileId = listing.TeacherProfileId,
            SubjectId = listing.SubjectId,
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
            CreatedAt = listing.CreatedAt
        };
    }

    public async Task<MyTutorListingDto?> UpdateMyListingAsync(
        Guid userId, Guid listingId, UpdateMyTutorListingDto request)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(listingId, userId);
        if (listing is null) return null;

        listing.SubjectId = request.SubjectId;
        listing.CityId = request.CityId;
        listing.DistrictId = request.DistrictId;
        listing.NeighborhoodId = request.NeighborhoodId;
        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Category = request.Category.Trim();
        listing.SubCategory = request.SubCategory.Trim();
        listing.ServiceType = request.ServiceType;
        listing.LessonDuration = request.LessonDuration;
        listing.Price = request.Price;
        listing.IsActive = request.IsActive;
        listing.UpdatedAt = DateTime.UtcNow;

        await _listingRepository.SaveChangesAsync();

        return new MyTutorListingDto
        {
            Id = listing.Id,
            TeacherProfileId = listing.TeacherProfileId,
            SubjectId = listing.SubjectId,
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

    // =====================================================
    // Tutor Certificates
    // =====================================================

    public async Task<List<TutorProfileCertificateDto>> GetMyCertificatesAsync(Guid userId)
    {
        var profile = await _teacherRepository.GetByUserIdWithCertificatesAsync(userId);
        if (profile is null) return new List<TutorProfileCertificateDto>();

        return profile.Certificates.Select(c => new TutorProfileCertificateDto
        {
            Id = c.Id,
            Name = c.Name,
            Organization = c.Organization,
            Year = c.Year,
            FileUrl = c.FileUrl,
            FileName = c.FileName,
            ContentType = c.ContentType
        }).ToList();
    }

    public async Task<TutorProfileCertificateDto> AddMyCertificateAsync(
        Guid userId, AddTutorCertificateDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Öğretmen profili bulunamadı.");

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

    public async Task<bool> DeleteMyCertificateAsync(Guid userId, Guid certificateId)
    {
        var certificate = await _teacherRepository.GetCertificateForUserAsync(userId, certificateId);
        if (certificate is null) return false;

        _teacherRepository.RemoveCertificate(certificate);
        await _teacherRepository.SaveChangesAsync();
        return true;
    }

    // =====================================================
    // Tutor Availability
    // =====================================================

    public async Task<List<TutorProfileAvailabilityDto>> GetMyAvailabilityAsync(Guid userId)
    {
        var profile = await _teacherRepository.GetByUserIdWithAvailabilitiesAsync(userId);
        if (profile is null) return new List<TutorProfileAvailabilityDto>();

        return profile.Availabilities.Select(a => new TutorProfileAvailabilityDto
        {
            Id = a.Id,
            Day = a.Day,
            Start = a.Start,
            End = a.End
        }).ToList();
    }

    public async Task<List<TutorProfileAvailabilityDto>> UpdateMyAvailabilityAsync(
        Guid userId, UpdateTutorAvailabilityDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Öğretmen profili bulunamadı.");

        var newAvailabilities = request.Items.Select(i => new TeacherAvailability
        {
            TeacherProfileId = profile.Id,
            Day = i.Day.Trim(),
            Start = i.Start.Trim(),
            End = i.End.Trim()
        }).ToList();

        await _teacherRepository.ReplaceAvailabilitiesAsync(profile.Id, newAvailabilities);
        await _teacherRepository.SaveChangesAsync();

        return newAvailabilities.Select(a => new TutorProfileAvailabilityDto
        {
            Id = a.Id,
            Day = a.Day,
            Start = a.Start,
            End = a.End
        }).ToList();
    }

    public async Task<bool> DeleteMyAvailabilityAsync(Guid userId, Guid availabilityId)
    {
        var availability = await _teacherRepository
            .GetAvailabilityForUserAsync(userId, availabilityId);

        if (availability is null) return false;

        _teacherRepository.RemoveAvailability(availability);
        await _teacherRepository.SaveChangesAsync();
        return true;
    }

    // =====================================================
    // Tutor Students
    // =====================================================

    public async Task<List<MyStudentDto>> GetMyStudentsAsync(Guid userId)
    {
        var bookings = await _bookingRepository.GetCompletedByTutorUserIdAsync(userId);

        return bookings
            .GroupBy(b => b.StudentUserId)
            .Select(g =>
            {
                var last = g.OrderByDescending(b => b.StartTime).First();
                return new MyStudentDto
                {
                    StudentId = g.Key,
                    StudentName = last.StudentUser?.FullName ?? "Bilinmiyor",
                    TeacherListingId = last.TeacherListingId,
                    LessonTitle = last.TeacherListing?.Title ?? "Ders",
                    LastLessonDate = last.StartTime,
                    Source = "Booking",
                    CompletedLessonCount = g.Count()
                };
            })
            .OrderByDescending(x => x.LastLessonDate)
            .ToList();
    }
}