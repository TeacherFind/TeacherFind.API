using Microsoft.AspNetCore.Http;
using System;
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
            PhoneNumber = x.TeacherProfile.User.PhoneNumber,
            Title = x.Title,
            Description = x.Description,
            Price = x.Price,
            City = x.City?.Name,
            District = x.District?.Name,
            Neighborhood = x.Neighborhood?.Name,
            Subject = x.Subject?.Name,
            Rating = x.TeacherProfile.Rating,
            ReviewCount = x.TeacherProfile.TotalReviews,
            IsFavorite = favoriteIds.Contains(x.Id),
            Photos = MapPhotos(x.Photos)
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

        var averageRating = reviews.Count > 0 ? reviews.Average(x => x.Rating) : 0;

        var profile = listing.TeacherProfile;

        return new TutorDetailDto
        {
            Id = listing.Id,
            TeacherProfileId = listing.TeacherProfileId,
            TeacherName = profile.User.FullName,
            PhoneNumber = profile.User.PhoneNumber,
            AvatarUrl = profile.User.ProfileImageUrl,
            Title = listing.Title,
            Bio = listing.Description,
            Price = listing.Price,
            LessonDuration = listing.LessonDuration,
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
            Photos = MapPhotos(listing.Photos),

            Reviews = reviews.Select(x => new TutorReviewDto
            {
                Id = x.Id,
                UserId = x.UserId,
                ReviewerName = x.Reviewer?.FullName ?? "Anonim",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt
            }).ToList(),

            Availability = profile.Availabilities
                .OrderBy(x => GetDayOrder(x.Day))
                .ThenBy(x => x.Start)
                .Select(x => new TutorAvailabilityDto
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

    // =====================================================
    // Tutor Profile
    // =====================================================

    public async Task<TutorProfileDto?> GetMyProfileAsync(Guid userId)
    {
        var profile = await _teacherRepository.GetByUserIdWithFullDetailsAsync(userId);

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

            Availabilities = profile.Availabilities
                .OrderBy(a => GetDayOrder(a.Day))
                .ThenBy(a => a.Start)
                .Select(a => new TutorProfileAvailabilityDto
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

        if (profile is null)
            return false;

        if (request.Headline is not null) profile.Headline = request.Headline.Trim();
        if (request.Bio is not null) profile.Bio = request.Bio.Trim();
        if (request.TeachingStyle is not null) profile.TeachingStyle = request.TeachingStyle.Trim();
        if (request.City is not null) profile.City = request.City.Trim();
        if (request.UniversityId is not null) profile.UniversityId = request.UniversityId;
        if (request.DepartmentId is not null) profile.DepartmentId = request.DepartmentId;

        profile.UpdatedAt = DateTime.UtcNow;

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
        return listings.Select(MapToMyTutorListingDto).ToList();
    }

    public async Task<MyTutorListingDto> CreateMyListingAsync(
        Guid userId, CreateMyTutorListingDto request)
    {
        var profile = await _teacherRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("Öğretmen profili bulunamadı.");

        var branchAlreadyExists = await _listingRepository.ExistsForTeacherBranchAsync(
            userId, request.SubjectId, request.Category, request.SubCategory);

        if (branchAlreadyExists)
            throw new InvalidOperationException("Bu branş için zaten bir ilanınız var.");

        var normalizedPrice = NormalizePrice(request.Price);

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
            LessonDuration = request.LessonDuration,
            Price = normalizedPrice,
            Status = "Pending",
            IsActive = true,
            IsApproved = false,
            ViewCount = 0
        };

        await _listingRepository.AddAsync(listing);
        await _listingRepository.SaveChangesAsync();

        return MapToMyTutorListingDto(listing);
    }

    public async Task<MyTutorListingDto?> UpdateMyListingAsync(
        Guid userId, Guid listingId, UpdateMyTutorListingDto request)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(listingId, userId);
        if (listing is null) return null;

        var branchAlreadyExists = await _listingRepository.ExistsForTeacherBranchAsync(
            userId, request.SubjectId, request.Category, request.SubCategory, listing.Id);

        if (branchAlreadyExists)
            throw new InvalidOperationException("Bu branş için zaten başka bir ilanınız var.");

        var normalizedPrice = NormalizePrice(request.Price);

        listing.SubjectId = request.SubjectId;
        listing.CityId = request.CityId;
        listing.DistrictId = request.DistrictId;
        listing.NeighborhoodId = request.NeighborhoodId;
        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.Category = request.Category.Trim();
        listing.SubCategory = request.SubCategory.Trim();
        listing.LessonDuration = request.LessonDuration;
        listing.Price = normalizedPrice;
        listing.IsActive = request.IsActive;
        listing.UpdatedAt = DateTime.UtcNow;
        listing.IsApproved = false;
        listing.Status = "Pending";

        await _listingRepository.SaveChangesAsync();

        return MapToMyTutorListingDto(listing);
    }

    public async Task<List<ListingPhotoDto>> UploadListingPhotosAsync(
        Guid userId, Guid listingId, List<IFormFile> files)
    {
        if (files is null || files.Count == 0)
            throw new InvalidOperationException("En az bir fotoğraf yüklenmelidir.");

        var listing = await _listingRepository.GetByIdForOwnerAsync(listingId, userId)
            ?? throw new InvalidOperationException("İlan bulunamadı veya bu ilana erişim yetkiniz yok.");

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp" };

        const long maxFileSize = 5 * 1024 * 1024;

        var uploadedPhotos = new List<ListingPhotoDto>();

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "uploads", "listings");

        Directory.CreateDirectory(uploadsFolder);

        foreach (var file in files)
        {
            if (file is null || file.Length == 0) continue;

            var extension = Path.GetExtension(file.FileName);

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Sadece .jpg, .jpeg, .png veya .webp fotoğraf yüklenebilir.");

            if (file.Length > maxFileSize)
                throw new InvalidOperationException("Her fotoğraf en fazla 5 MB olabilir.");

            var storedFileName = $"{listingId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var photo = new ListingPhoto
            {
                ListingId = listingId,
                PhotoUrl = $"/uploads/listings/{storedFileName}",
                IsMain = !listing.Photos.Any()
            };

            listing.Photos.Add(photo);
            uploadedPhotos.Add(new ListingPhotoDto
            {
                Id = photo.Id,
                PhotoUrl = photo.PhotoUrl,
                IsMain = photo.IsMain
            });
        }

        if (uploadedPhotos.Count == 0)
            throw new InvalidOperationException("Geçerli fotoğraf bulunamadı.");

        await _listingRepository.SaveChangesAsync();

        return uploadedPhotos;
    }

    // =====================================================
    // Tutor Certificates
    // =====================================================

    public async Task<List<TutorProfileCertificateDto>> GetMyCertificatesAsync(Guid userId)
    {
        var profile = await _teacherRepository.GetByUserIdWithCertificatesAsync(userId);
        if (profile is null) return new List<TutorProfileCertificateDto>();

        return profile.Certificates
            .OrderByDescending(c => c.Year)
            .Select(c => new TutorProfileCertificateDto
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

        return profile.Availabilities
            .OrderBy(a => GetDayOrder(a.Day)).ThenBy(a => a.Start)
            .Select(a => new TutorProfileAvailabilityDto
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

        var availabilities = request.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.Day) &&
                        !string.IsNullOrWhiteSpace(i.Start) &&
                        !string.IsNullOrWhiteSpace(i.End))
            .Select(i => new TeacherAvailability
            {
                TeacherProfileId = profile.Id,
                Day = i.Day.Trim(),
                Start = i.Start.Trim(),
                End = i.End.Trim()
            }).ToList();

        await _teacherRepository.ReplaceAvailabilitiesAsync(profile.Id, availabilities);
        await _teacherRepository.SaveChangesAsync();

        return availabilities
            .OrderBy(a => GetDayOrder(a.Day)).ThenBy(a => a.Start)
            .Select(a => new TutorProfileAvailabilityDto
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
            .Select(group =>
            {
                var last = group.OrderByDescending(b => b.StartTime).First();
                return new MyStudentDto
                {
                    StudentId = group.Key,
                    StudentName = last.StudentUser?.FullName ?? "Bilinmiyor",
                    TeacherListingId = last.TeacherListingId,
                    LessonTitle = last.TeacherListing?.Title ?? "Ders",
                    LastLessonDate = last.StartTime,
                    Source = last.Source.ToString(),
                    CompletedLessonCount = group.Count()
                };
            })
            .OrderByDescending(x => x.LastLessonDate)
            .ToList();
    }

    // =====================================================
    // Earnings Report
    // =====================================================

    public async Task<TutorEarningsReportDto> GetEarningsReportAsync(
        Guid tutorUserId, DateTime from, DateTime to)
    {
        var bookings = await _bookingRepository.GetCompletedByTutorUserIdAsync(tutorUserId);

        var inRange = bookings
            .Where(b => b.StartTime.Date >= from.Date && b.StartTime.Date <= to.Date)
            .ToList();

        var items = inRange.Select(b => new TutorEarningsItemDto
        {
            BookingId = b.Id,
            LessonTitle = b.TeacherListing?.Title ?? "Ders",
            StudentName = b.StudentUser?.FullName ?? "Bilinmiyor",
            CompletedAt = b.StartTime,
            Amount = b.TeacherListing?.Price ?? 0
        }).ToList();

        return new TutorEarningsReportDto
        {
            TotalEarnings = items.Sum(x => x.Amount),
            CompletedLessonCount = items.Count,
            From = from.ToString("yyyy-MM-dd"),
            To = to.ToString("yyyy-MM-dd"),
            Items = items
        };
    }

    // =====================================================
    // Photo Management
    // =====================================================

    public async Task<bool> SetMainListingPhotoAsync(
        Guid tutorUserId, Guid listingId, Guid photoId)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(listingId, tutorUserId);
        if (listing is null) return false;

        var photos = await _listingRepository.GetPhotosByListingIdAsync(listingId);

        if (!photos.Any(p => p.Id == photoId))
            return false;

        foreach (var photo in photos)
        {
            photo.IsMain = photo.Id == photoId;
            photo.UpdatedAt = DateTime.UtcNow;
        }

        await _listingRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateListingPhotoSortOrderAsync(
        Guid tutorUserId, Guid listingId, UpdateListingPhotoSortOrderDto request)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(listingId, tutorUserId);
        if (listing is null) return false;

        var photos = await _listingRepository.GetPhotosByListingIdAsync(listingId);

        for (var i = 0; i < request.PhotoIds.Count; i++)
        {
            var photo = photos.FirstOrDefault(p => p.Id == request.PhotoIds[i]);
            if (photo is not null)
            {
                photo.SortOrder = i;
                photo.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _listingRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteListingPhotoAsync(
        Guid tutorUserId, Guid listingId, Guid photoId)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(listingId, tutorUserId);
        if (listing is null) return false;

        var photos = await _listingRepository.GetPhotosByListingIdAsync(listingId);
        var photo = photos.FirstOrDefault(p => p.Id == photoId);
        if (photo is null) return false;

        await _listingRepository.RemovePhotoAsync(photo);
        await _listingRepository.SaveChangesAsync();
        return true;
    }
    // =====================================================
    // Helpers
    // =====================================================

    private static List<ListingPhotoDto> MapPhotos(IEnumerable<ListingPhoto>? photos)
    {
        return photos?
            .OrderByDescending(p => p.IsMain)
            .ThenBy(p => p.CreatedAt)
            .Select(p => new ListingPhotoDto
            {
                Id = p.Id,
                PhotoUrl = p.PhotoUrl,
                IsMain = p.IsMain
            }).ToList() ?? new List<ListingPhotoDto>();
    }

    private static decimal NormalizePrice(decimal price)
    {
        var normalizedPrice = Math.Round(price / 50m, MidpointRounding.AwayFromZero) * 50m;

        if (normalizedPrice < 300 || normalizedPrice > 5000)
            throw new InvalidOperationException("Fiyat 300 TL ile 5000 TL arasında olmalıdır.");

        return normalizedPrice;
    }

    private static int GetDayOrder(string day)
    {
        return day.Trim().ToLowerInvariant() switch
        {
            "monday" or "pazartesi" => 1,
            "tuesday" or "salı" => 2,
            "wednesday" or "çarşamba" => 3,
            "thursday" or "perşembe" => 4,
            "friday" or "cuma" => 5,
            "saturday" or "cumartesi" => 6,
            "sunday" or "pazar" => 7,
            _ => 99
        };
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
            Title = listing.Title,
            Description = listing.Description,
            Category = listing.Category,
            SubCategory = listing.SubCategory,
            LessonDuration = listing.LessonDuration,
            Price = listing.Price,
            Status = listing.Status,
            IsActive = listing.IsActive,
            IsApproved = listing.IsApproved,
            ViewCount = listing.ViewCount,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            Photos = MapPhotos(listing.Photos)
        };
    }
}