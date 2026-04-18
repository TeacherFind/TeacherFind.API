using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Listings;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Services;

public interface IListingService
{
    Task<List<ListingDto>> GetListingsAsync();

    Task<ListingDetailDto?> GetByIdAsync(Guid id);

    Task CreateListingAsync(CreateListingRequestDto request);

    Task<List<ListingDto>> FilterAsync(ListingFilterRequestDto filter);

    Task<bool> UpdateListingAsync(Guid id, UpdateListingRequestDto request);
    Task<bool> DeleteListingAsync(Guid id);

    //  Yeni eklenen metod
    Task<PagedResultDto<ListingDto>> GetPagedAsync(PagedRequestDto request);

}