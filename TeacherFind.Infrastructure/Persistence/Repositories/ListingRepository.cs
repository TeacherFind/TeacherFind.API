using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Contracts.Listings;
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
            .ToListAsync();
    }

    public async Task<TeacherListing?> GetByIdAsync(Guid id)
    {
        return await _context.TeacherListings
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TeacherListing?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.TeacherListings
            .Include(x => x.TeacherProfile)
                .ThenInclude(tp => tp.User)
            .Include(x => x.TeacherProfile.Certificates)
            .Include(x => x.TeacherProfile.Availabilities)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TeacherListing?> GetByIdForOwnerAsync(Guid id, Guid userId)
    {
        return await _context.TeacherListings
            .Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x => x.Id == id && x.TeacherProfile.UserId == userId);
    }

    public async Task<TeacherProfile?> GetTeacherProfileByUserIdAsync(Guid userId)
    {
        return await _context.TeacherProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(TeacherListing listing)
    {
        await _context.TeacherListings.AddAsync(listing);
    }

    public async Task<List<TeacherListing>> FilterAsync(ListingFilterRequestDto filter)
    {
        var query = _context.TeacherListings.AsQueryable();

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

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}