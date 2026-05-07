using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Contracts.Listings;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly AppDbContext _context;

    public ListingRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<TeacherListing> Query()
    {
        return _context.TeacherListings.AsQueryable();
    }

    public async Task<List<TeacherListing>> GetAllAsync()
    {
        return await _context.TeacherListings
            .AsNoTracking()
            .Include(x => x.Photos)
            .ToListAsync();
    }

    public async Task<TeacherListing?> GetByIdAsync(Guid id)
    {
        return await _context.TeacherListings
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TeacherListing?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.TeacherListings
            .Include(x => x.TeacherProfile)
                .ThenInclude(tp => tp.User)
            .Include(x => x.TeacherProfile)
                .ThenInclude(tp => tp.Certificates)
            .Include(x => x.TeacherProfile)
                .ThenInclude(tp => tp.Availabilities)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TeacherListing?> GetByIdWithFullDetailsAsync(Guid id)
    {
        return await _context.TeacherListings
            .Include(x => x.TeacherProfile)
                .ThenInclude(p => p.User)
            .Include(x => x.TeacherProfile)
                .ThenInclude(p => p.University)
            .Include(x => x.TeacherProfile)
                .ThenInclude(p => p.DepartmentEntity)
            .Include(x => x.TeacherProfile)
                .ThenInclude(p => p.Certificates)
            .Include(x => x.TeacherProfile)
                .ThenInclude(p => p.Availabilities)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.Neighborhood)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.IsApproved);
    }

    public async Task<TeacherListing?> GetByIdForOwnerAsync(Guid id, Guid userId)
    {
        return await _context.TeacherListings
            .Include(x => x.TeacherProfile)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.TeacherProfile.UserId == userId);
    }

    public async Task<TeacherProfile?> GetTeacherProfileByUserIdAsync(Guid userId)
    {
        return await _context.TeacherProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.University)
            .Include(x => x.DepartmentEntity)
            .Include(x => x.Certificates)
            .Include(x => x.Availabilities)
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(TeacherListing listing)
    {
        await _context.TeacherListings.AddAsync(listing);
    }

    public async Task<List<TeacherListing>> FilterAsync(ListingFilterRequestDto filter)
    {
        var query = _context.TeacherListings
            .Include(x => x.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x =>
                x.Title.Contains(filter.Search) ||
                x.Description.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= filter.MaxPrice.Value);
        }

        if (filter.ServiceType.HasValue)
        {
            query = query.Where(x => x.ServiceType == filter.ServiceType.Value);
        }

        if (filter.OnlyApproved == true)
        {
            query = query.Where(x => x.IsApproved);
        }

        return await query.ToListAsync();
    }

    public async Task<(List<TeacherListing> Items, int TotalCount)> FilterTutorsAsync(
        TutorFilterRequestDto filter)
    {
        var query = _context.TeacherListings
            .Where(x => x.IsActive && x.IsApproved)
            .Include(x => x.TeacherProfile)
                .ThenInclude(p => p.User)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.Neighborhood)
            .Include(x => x.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x =>
                x.Title.Contains(filter.Search) ||
                x.Description.Contains(filter.Search) ||
                x.TeacherProfile.User.FullName.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(x => x.Category == filter.Category);
        }

        if (filter.SubjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == filter.SubjectId.Value);
        }

        if (filter.CityId.HasValue)
        {
            query = query.Where(x => x.CityId == filter.CityId.Value);
        }

        if (filter.DistrictId.HasValue)
        {
            query = query.Where(x => x.DistrictId == filter.DistrictId.Value);
        }

        if (filter.NeighborhoodId.HasValue)
        {
            query = query.Where(x => x.NeighborhoodId == filter.NeighborhoodId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= filter.MaxPrice.Value);
        }

        if (filter.ServiceType.HasValue)
        {
            query = query.Where(x => x.ServiceType == filter.ServiceType.Value);
        }

        query = filter.Sort switch
        {
            "price_asc" => query.OrderBy(x => x.Price),
            "price_desc" => query.OrderByDescending(x => x.Price),
            "rating" => query.OrderByDescending(x => x.TeacherProfile.Rating),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<TeacherListing>> GetByTeacherUserIdAsync(Guid userId)
    {
        return await _context.TeacherListings
            .AsNoTracking()
            .Include(x => x.TeacherProfile)
                .ThenInclude(x => x.User)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.Neighborhood)
            .Include(x => x.Photos)
            .Where(x => x.TeacherProfile.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<TeacherListing?> GetByIdForTeacherUserAsync(
        Guid listingId,
        Guid userId)
    {
        return await _context.TeacherListings
            .Include(x => x.TeacherProfile)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.Neighborhood)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x =>
                x.Id == listingId &&
                x.TeacherProfile.UserId == userId);
    }

    public async Task<bool> ExistsForTeacherBranchAsync(
        Guid teacherUserId,
        int? subjectId,
        string category,
        string subCategory,
        Guid? excludeListingId = null)
    {
        var query = _context.TeacherListings
            .Include(x => x.TeacherProfile)
            .Where(x => x.TeacherProfile.UserId == teacherUserId);

        if (excludeListingId.HasValue)
        {
            query = query.Where(x => x.Id != excludeListingId.Value);
        }

        if (subjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == subjectId.Value);
        }
        else
        {
            var normalizedCategory = category.Trim();
            var normalizedSubCategory = subCategory.Trim();

            query = query.Where(x =>
                x.Category == normalizedCategory &&
                x.SubCategory == normalizedSubCategory);
        }

        return await query.AnyAsync();
    }

    public async Task<List<ListingPhoto>> GetPhotosByListingIdAsync(Guid listingId)
    {
        return await _context.Set<ListingPhoto>()
            .Where(p => p.ListingId == listingId)
            .OrderByDescending(p => p.IsMain)
            .ThenBy(p => p.SortOrder)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public Task RemovePhotoAsync(ListingPhoto photo)
    {
        _context.Set<ListingPhoto>().Remove(photo);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}