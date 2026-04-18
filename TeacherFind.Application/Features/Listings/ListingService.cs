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

    // 🔹 TÜM İLANLAR
    public async Task<List<ListingDto>> GetListingsAsync()
    {
        var listings = await _listingRepository.GetAllAsync();

        return listings.Select(x => new ListingDto
        {
            Id = x.Id,
            TeacherProfileId = x.TeacherProfileId,
            Title = x.Title,
            Description = x.Description,
            Price = x.Price,
            IsActive = x.IsActive,
            IsApproved = x.IsApproved,
            ViewCount = x.ViewCount
        }).ToList();
    }

    // 🔹 TEK İLAN + VIEW COUNT
    public async Task<ListingDetailDto?> GetByIdAsync(Guid id)
    {
        var listing = await _listingRepository.GetByIdWithDetailsAsync(id);

        if (listing == null)
            return null;

        // 🔥 VIEW COUNT ARTIR
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
                }).ToList(),

            MusaitGunler = listing.TeacherProfile.Availabilities
                .Select(a => new AvailabilityDto
                {
                    Gun = a.Day,
                    Baslangic = a.Start,
                    Bitis = a.End
                }).ToList(),

            IlanDurumu = listing.Status,
            OlusturulmaTarihi = listing.CreatedAt,
            GuncellemeTarihi = listing.UpdatedAt
        };
    }

    // 🔹 İLAN OLUŞTUR
    public async Task CreateListingAsync(CreateListingRequestDto request)
    {
        var listing = new TeacherListing
        {
            TeacherProfileId = request.TeacherProfileId,
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            SubCategory = request.SubCategory,
            LessonDuration = request.LessonDuration,
            Price = request.Price,
            ServiceType = request.ServiceType,
            Status = "Pending",
            ViewCount = 0,
            IsActive = true
        };

        await _listingRepository.AddAsync(listing);
        await _listingRepository.SaveChangesAsync();
    }

    // 🔹 FİLTRE
    public async Task<List<ListingDto>> FilterAsync(ListingFilterRequestDto filter)
    {
        var listings = await _listingRepository.FilterAsync(filter);

        return listings.Select(x => new ListingDto
        {
            Id = x.Id,
            TeacherProfileId = x.TeacherProfileId,
            Title = x.Title,
            Description = x.Description,
            Price = x.Price,
            IsActive = x.IsActive,
            IsApproved = x.IsApproved,
            ViewCount = x.ViewCount
        }).ToList();
    }

    // 🔹 UPDATE
    public async Task<bool> UpdateListingAsync(Guid id, UpdateListingRequestDto request)
    {
        var listing = await _listingRepository.GetByIdAsync(id);

        if (listing == null)
            return false;

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Price = request.Price;

        await _listingRepository.SaveChangesAsync();

        return true;
    }

    // 🔹 DELETE (SOFT DELETE)
    public async Task<bool> DeleteListingAsync(Guid id)
    {
        var listing = await _listingRepository.GetByIdAsync(id);

        if (listing == null)
            return false;

        listing.IsActive = false;

        await _listingRepository.SaveChangesAsync();

        return true;
    }

    public async Task<PagedResultDto<ListingDto>> GetPagedAsync(PagedRequestDto request)
    {
        var query = (await _listingRepository.GetAllAsync()).AsQueryable();

        // 🔥 SORTING
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            if (request.SortBy.ToLower() == "price")
            {
                query = request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.Price)
                    : query.OrderBy(x => x.Price);
            }
            else if (request.SortBy.ToLower() == "viewcount")
            {
                query = request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.ViewCount)
                    : query.OrderBy(x => x.ViewCount);
            }
        }

        var totalCount = query.Count();

        // 🔥 PAGINATION
        var items = query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ListingDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Price = x.Price,
                ViewCount = x.ViewCount
            })
            .ToList();

        return new PagedResultDto<ListingDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}