using TeacherFind.Contracts.Listings;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IListingRepository
{
    Task<List<TeacherListing>> GetAllAsync();
    Task<TeacherListing?> GetByIdAsync(Guid id);
    Task<TeacherListing?> GetByIdWithDetailsAsync(Guid id);
    Task<TeacherListing?> GetByIdWithFullDetailsAsync(Guid id);          // NEW — Task 6
    Task<TeacherListing?> GetByIdForOwnerAsync(Guid id, Guid userId);
    Task<TeacherProfile?> GetTeacherProfileByUserIdAsync(Guid userId);

    IQueryable<TeacherListing> Query();

    Task AddAsync(TeacherListing listing);
    Task<List<TeacherListing>> FilterAsync(ListingFilterRequestDto filter);
    Task<(List<TeacherListing> Items, int TotalCount)> FilterTutorsAsync(TutorFilterRequestDto filter); // NEW — Task 5
    Task SaveChangesAsync();
}