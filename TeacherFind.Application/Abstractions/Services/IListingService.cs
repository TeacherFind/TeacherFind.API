using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Listings;

namespace TeacherFind.Application.Abstractions.Services;

public interface IListingService
{
    Task<ListingDetailDto?> GetByIdAsync(Guid id);

    Task CreateListingAsync(CreateListingRequestDto request, Guid userId);

    Task<PagedResultDto<ListingDto>> FilterAsync(ListingFilterRequestDto request);

    Task<bool> UpdateListingAsync(Guid id, UpdateListingRequestDto request, Guid userId);

    Task<bool> DeleteListingAsync(Guid id, Guid userId);
}