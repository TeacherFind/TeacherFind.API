using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Listings;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Listings;

public class ListingService : IListingService
{
    private readonly IListingRepository _listingRepository;

    public ListingService(IListingRepository listingRepository)
    {
        _listingRepository = listingRepository;
    }

    public async Task<ListingDetailDto?> GetByIdAsync(Guid id)
    {
        var listing = await _listingRepository.GetByIdWithDetailsAsync(id);

        if (listing == null)
            return null;

        listing.ViewCount += 1;
        await _listingRepository.SaveChangesAsync();

        return new ListingDetailDto
        {
            IlanId = listing.Id,
            IlanBasligi = listing.Title,
            IlanAciklamasi = listing.Description,
            Kategori = listing.Category,
            AltKategori = listing.SubCategory,
            DersTipi = listing.ServiceType.ToString(),
            DersSuresi = listing.LessonDuration,
            SaatlikFiyat = listing.Price,
            ViewCount = listing.ViewCount,

            OgretmenId = listing.TeacherProfile.UserId,
            OgretmenAdi = listing.TeacherProfile.User.FullName,
            OgretmenFotografi = listing.TeacherProfile.User.ProfileImageUrl,
            OgretmenBasligi = listing.TeacherProfile.Title,
            OgretmenBio = listing.TeacherProfile.Bio,
            OgretmenSehir = listing.TeacherProfile.City,
            OgretmenPuani = listing.TeacherProfile.Rating,
            YorumSayisi = listing.TeacherProfile.TotalReviews,

            Sertifikalar = listing.TeacherProfile.Certificates
                .Select(c => new CertificateDto
                {
                    SertifikaAdi = c.Name,
                    VerenKurum = c.Organization,
                    Yil = c.Year
                })
                .ToList(),

            MusaitGunler = listing.TeacherProfile.Availabilities
                .Select(a => new AvailabilityDto
                {
                    Gun = a.Day,
                    Baslangic = a.Start,
                    Bitis = a.End
                })
                .ToList(),

            IlanDurumu = listing.Status,
            OlusturulmaTarihi = listing.CreatedAt,
            GuncellemeTarihi = listing.UpdatedAt
        };
    }

    public async Task CreateListingAsync(CreateListingRequestDto request, Guid userId)
    {
        var teacherProfile = await _listingRepository.GetTeacherProfileByUserIdAsync(userId);

        if (teacherProfile == null)
            throw new Exception("Öğretmen profili bulunamadı.");

        var listing = new TeacherListing
        {
            TeacherProfileId = teacherProfile.Id,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            SubCategory = request.SubCategory,
            LessonDuration = request.LessonDuration,
            Price = request.Price,
            ServiceType = request.ServiceType,
            Status = "Pending",
            ViewCount = 0,
            IsActive = true,
            IsApproved = false
        };

        await _listingRepository.AddAsync(listing);
        await _listingRepository.SaveChangesAsync();
    }

    public async Task<PagedResultDto<ListingDto>> FilterAsync(ListingFilterRequestDto filter)
    {
        var listings = await _listingRepository.FilterAsync(filter);
        var query = listings.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            if (string.Equals(filter.SortBy, "price", StringComparison.OrdinalIgnoreCase))
            {
                query = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(x => x.Price)
                    : query.OrderBy(x => x.Price);
            }
            else if (string.Equals(filter.SortBy, "viewcount", StringComparison.OrdinalIgnoreCase))
            {
                query = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(x => x.ViewCount)
                    : query.OrderBy(x => x.ViewCount);
            }
            else if (string.Equals(filter.SortBy, "createdat", StringComparison.OrdinalIgnoreCase))
            {
                query = string.Equals(filter.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        var totalCount = query.Count();

        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new ListingDto
            {
                Id = x.Id,
                TeacherProfileId = x.TeacherProfileId,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                IsActive = x.IsActive,
                IsApproved = x.IsApproved,
                ViewCount = x.ViewCount
            })
            .ToList();

        return new PagedResultDto<ListingDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<bool> UpdateListingAsync(Guid id, UpdateListingRequestDto request, Guid userId)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(id, userId);

        if (listing == null)
            return false;

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Price = request.Price;
        listing.UpdatedAt = DateTime.UtcNow;

        await _listingRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteListingAsync(Guid id, Guid userId)
    {
        var listing = await _listingRepository.GetByIdForOwnerAsync(id, userId);

        if (listing == null)
            return false;

        listing.IsActive = false;
        listing.UpdatedAt = DateTime.UtcNow;

        await _listingRepository.SaveChangesAsync();
        return true;
    }
}