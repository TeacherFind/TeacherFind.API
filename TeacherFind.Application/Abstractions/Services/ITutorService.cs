using Microsoft.AspNetCore.Http;
using TeacherFind.Contracts.Common;
using TeacherFind.Contracts.Tutors;

namespace TeacherFind.Application.Abstractions.Services;

public interface ITutorService
{
    Task<PagedResultDto<TutorListItemDto>> GetTutorsAsync(
        TutorFilterRequestDto filter,
        Guid? currentUserId);

    Task<TutorDetailDto?> GetTutorByIdAsync(
        Guid listingId,
        Guid? currentUserId);

    Task<TutorProfileDto?> GetMyProfileAsync(Guid userId);

    Task<bool> UpdateMyProfileAsync(
        Guid currentUserId,
        UpdateTutorProfileDto request);

    Task<List<MyTutorListingDto>> GetMyListingsAsync(Guid currentUserId);

    Task<MyTutorListingDto> CreateMyListingAsync(
        Guid currentUserId,
        CreateMyTutorListingDto request);

    Task<MyTutorListingDto?> UpdateMyListingAsync(
        Guid currentUserId,
        Guid listingId,
        UpdateMyTutorListingDto request);

    Task<List<ListingPhotoDto>> UploadListingPhotosAsync(
        Guid currentUserId,
        Guid listingId,
        List<IFormFile> files);

    Task<List<MyStudentDto>> GetMyStudentsAsync(Guid currentUserId);

    Task<List<TutorProfileCertificateDto>> GetMyCertificatesAsync(Guid currentUserId);

    Task<TutorProfileCertificateDto> AddMyCertificateAsync(
        Guid currentUserId,
        AddTutorCertificateDto request);

    Task<bool> DeleteMyCertificateAsync(
        Guid currentUserId,
        Guid certificateId);

    Task<List<TutorProfileAvailabilityDto>> GetMyAvailabilityAsync(Guid currentUserId);

    Task<List<TutorProfileAvailabilityDto>> UpdateMyAvailabilityAsync(
        Guid currentUserId,
        UpdateTutorAvailabilityDto request);

    Task<bool> DeleteMyAvailabilityAsync(
        Guid currentUserId,
        Guid availabilityId);
    Task<TutorEarningsReportDto> GetEarningsReportAsync(
    Guid tutorUserId,
    DateTime from,
    DateTime to);
    Task<bool> SetMainListingPhotoAsync(Guid tutorUserId, Guid listingId, Guid photoId);
    Task<bool> UpdateListingPhotoSortOrderAsync(Guid tutorUserId, Guid listingId, UpdateListingPhotoSortOrderDto request);
    Task<bool> DeleteListingPhotoAsync(Guid tutorUserId, Guid listingId, Guid photoId);
}