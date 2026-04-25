using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Tutors;

namespace TeacherFind.Application.Abstractions.Services;

public interface ITutorService
{
    Task<PagedResultDto<TutorListItemDto>> GetTutorsAsync(TutorFilterRequestDto filter, Guid? currentUserId);
    Task<TutorDetailDto?> GetTutorByIdAsync(Guid listingId, Guid? currentUserId);
}
