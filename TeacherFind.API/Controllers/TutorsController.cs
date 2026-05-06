using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Bookings;
using TeacherFind.Contracts.Tutors;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/tutors")]
public class TutorsController : ControllerBase
{
    private readonly ITutorService _tutorService;
    private readonly IBookingService _bookingService;
    private readonly IUserRepository _userRepository;

    public TutorsController(
        ITutorService tutorService,
        IBookingService bookingService,
        IUserRepository userRepository)
    {
        _tutorService = tutorService;
        _bookingService = bookingService;
        _userRepository = userRepository;
    }

    // =====================================================
    // Public Tutor Endpoints
    // =====================================================

    // GET /api/tutors
    [HttpGet]
    public async Task<IActionResult> GetTutors([FromQuery] TutorFilterRequestDto filter)
    {
        var result = await _tutorService.GetTutorsAsync(filter, GetCurrentUserId());

        return Ok(result);
    }

    // GET /api/tutors/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTutor(Guid id)
    {
        var result = await _tutorService.GetTutorByIdAsync(id, GetCurrentUserId());

        if (result is null)
            return NotFound(new { message = "İlan bulunamadı." });

        return Ok(result);
    }

    // =====================================================
    // Tutor Profile
    // =====================================================

    // GET /api/tutors/profile
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var profile = await _tutorService.GetMyProfileAsync(currentUserId);

        if (profile is null)
            return NotFound(new { message = "Öğretmen profili bulunamadı." });

        return Ok(profile);
    }

    // PUT /api/tutors/profile
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTutorProfileDto request)
    {
        if (request is null)
            return BadRequest(new { message = "Profil bilgileri gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.UpdateMyProfileAsync(currentUserId, request);

        if (!result)
            return BadRequest(new { message = "Profil güncellenemedi." });

        return Ok(new { message = "Profil başarıyla güncellendi." });
    }

    // POST /api/tutors/avatar
    [Authorize(Policy = "TutorOnly")]
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(IFormFile? file)
    {
        var currentUserId = GetRequiredCurrentUserId();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Dosya gönderilmedi." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Sadece .jpg, .jpeg, .png veya .webp dosyaları yüklenebilir." });

        const long maxFileSize = 2 * 1024 * 1024;

        if (file.Length > maxFileSize)
            return BadRequest(new { message = "Dosya boyutu en fazla 2 MB olabilir." });

        var user = await _userRepository.GetByIdAsync(currentUserId);

        if (user is null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "avatars");

        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{currentUserId}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var avatarUrl = $"/uploads/avatars/{fileName}";

        user.ProfileImageUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.SaveChangesAsync();

        return Ok(new
        {
            message = "Avatar başarıyla yüklendi.",
            profileImageUrl = avatarUrl
        });
    }

    // =====================================================
    // Tutor Listings
    // =====================================================

    // GET /api/tutors/my-listings
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("my-listings")]
    public async Task<IActionResult> GetMyListings()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.GetMyListingsAsync(currentUserId);

        return Ok(result);
    }

    // POST /api/tutors/my-listings
    [Authorize(Policy = "TutorOnly")]
    [HttpPost("my-listings")]
    public async Task<IActionResult> CreateMyListing([FromBody] CreateMyTutorListingDto request)
    {
        if (request is null)
            return BadRequest(new { message = "İlan bilgileri gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _tutorService.CreateMyListingAsync(currentUserId, request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/tutors/my-listings/{id}
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("my-listings/{id:guid}")]
    public async Task<IActionResult> UpdateMyListing(
        Guid id,
        [FromBody] UpdateMyTutorListingDto request)
    {
        if (request is null)
            return BadRequest(new { message = "İlan bilgileri gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _tutorService.UpdateMyListingAsync(
                currentUserId,
                id,
                request);

            if (result is null)
                return NotFound(new { message = "İlan bulunamadı veya bu ilana erişim yetkiniz yok." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/tutors/my-listings/{listingId}/photos
    [Authorize(Policy = "TutorOnly")]
    [HttpPost("my-listings/{listingId:guid}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadListingPhotos(
        Guid listingId,
        List<IFormFile> files)
    {
        var currentUserId = GetRequiredCurrentUserId();

        if (files is null || files.Count == 0)
            return BadRequest(new { message = "En az bir fotoğraf yüklenmelidir." });

        try
        {
            var result = await _tutorService.UploadListingPhotosAsync(
                currentUserId,
                listingId,
                files);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =====================================================
    // Tutor Certificates
    // =====================================================

    // GET /api/tutors/certificates
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("certificates")]
    public async Task<IActionResult> GetMyCertificates()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.GetMyCertificatesAsync(currentUserId);

        return Ok(result);
    }

    // POST /api/tutors/certificates
    [Authorize(Policy = "TutorOnly")]
    [HttpPost("certificates")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddCertificate(
        [FromForm] string name,
        [FromForm] string organization,
        [FromForm] int year,
        IFormFile? file)
    {
        var currentUserId = GetRequiredCurrentUserId();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Sertifika adı zorunludur." });

        if (string.IsNullOrWhiteSpace(organization))
            return BadRequest(new { message = "Kurum adı zorunludur." });

        if (year < 1950 || year > DateTime.UtcNow.Year + 1)
            return BadRequest(new { message = "Geçerli bir yıl giriniz." });

        try
        {
            var certificateFile = await SaveCertificateFileAsync(currentUserId, file);

            var result = await _tutorService.AddMyCertificateAsync(
                currentUserId,
                new AddTutorCertificateDto
                {
                    Name = name.Trim(),
                    Organization = organization.Trim(),
                    Year = year,
                    FileUrl = certificateFile.FileUrl,
                    FileName = certificateFile.FileName,
                    ContentType = certificateFile.ContentType
                });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/tutors/certificates/{id}
    [Authorize(Policy = "TutorOnly")]
    [HttpDelete("certificates/{id:guid}")]
    public async Task<IActionResult> DeleteCertificate(Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.DeleteMyCertificateAsync(currentUserId, id);

        if (!result)
            return NotFound(new { message = "Sertifika bulunamadı veya erişim yetkiniz yok." });

        return Ok(new { message = "Sertifika silindi." });
    }

    // =====================================================
    // Tutor Availability
    // =====================================================

    // GET /api/tutors/availability
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("availability")]
    public async Task<IActionResult> GetMyAvailability()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.GetMyAvailabilityAsync(currentUserId);

        return Ok(result);
    }

    // PUT /api/tutors/availability
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("availability")]
    public async Task<IActionResult> UpdateMyAvailability([FromBody] UpdateTutorAvailabilityDto request)
    {
        if (request is null || request.Items.Count == 0)
            return BadRequest(new { message = "En az bir müsaitlik aralığı gönderilmelidir." });

        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _tutorService.UpdateMyAvailabilityAsync(currentUserId, request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/tutors/availability/{id}
    [Authorize(Policy = "TutorOnly")]
    [HttpDelete("availability/{id:guid}")]
    public async Task<IActionResult> DeleteMyAvailability(Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.DeleteMyAvailabilityAsync(currentUserId, id);

        if (!result)
            return NotFound(new { message = "Müsaitlik kaydı bulunamadı veya erişim yetkiniz yok." });

        return Ok(new { message = "Müsaitlik kaydı silindi." });
    }

    // =====================================================
    // Tutor Bookings
    // =====================================================

    // GET /api/tutors/my-bookings
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _bookingService.GetTutorBookingsAsync(currentUserId);

        return Ok(result);
    }

    // PUT /api/tutors/my-bookings/{id}/approve
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("my-bookings/{id:guid}/approve")]
    public async Task<IActionResult> ApproveBooking(Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _bookingService.ApproveAsync(id, currentUserId);

            if (!result)
                return NotFound(new { message = "Rezervasyon bulunamadı veya erişim yetkiniz yok." });

            return Ok(new { message = "Rezervasyon onaylandı." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/tutors/my-bookings/{id}/reject
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("my-bookings/{id:guid}/reject")]
    public async Task<IActionResult> RejectBooking(
        Guid id,
        [FromBody] RejectBookingRequestDto request)
    {
        if (request is null)
            return BadRequest(new { message = "Ret bilgisi gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _bookingService.RejectAsync(id, currentUserId, request);

            if (!result)
                return NotFound(new { message = "Rezervasyon bulunamadı veya erişim yetkiniz yok." });

            return Ok(new { message = "Rezervasyon reddedildi." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/tutors/my-bookings/{id}/complete
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("my-bookings/{id:guid}/complete")]
    public async Task<IActionResult> CompleteBooking(Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _bookingService.CompleteAsync(id, currentUserId);

            if (!result)
                return NotFound(new { message = "Rezervasyon bulunamadı veya erişim yetkiniz yok." });

            return Ok(new { message = "Ders tamamlandı olarak işaretlendi." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // =====================================================
    // Tutor Students
    // =====================================================

    // GET /api/tutors/my-students
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.GetMyStudentsAsync(currentUserId);

        return Ok(result);
    }

    // =====================================================
    // Helpers
    // =====================================================

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out var userId)
            ? userId
            : null;
    }

    private Guid GetRequiredCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Geçersiz kullanıcı tokenı.");

        return userId;
    }

    private static async Task<UploadedFileResult> SaveCertificateFileAsync(
        Guid currentUserId,
        IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return new UploadedFileResult();

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Sadece PDF veya görsel dosyası yüklenebilir.");

        const long maxFileSize = 5 * 1024 * 1024;

        if (file.Length > maxFileSize)
            throw new InvalidOperationException("Dosya boyutu en fazla 5 MB olabilir.");

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "certificates");

        Directory.CreateDirectory(uploadsFolder);

        var storedFileName = $"{currentUserId}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, storedFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return new UploadedFileResult
        {
            FileUrl = $"/uploads/certificates/{storedFileName}",
            FileName = file.FileName,
            ContentType = file.ContentType
        };
    }

    private sealed class UploadedFileResult
    {
        public string? FileUrl { get; set; }

        public string? FileName { get; set; }

        public string? ContentType { get; set; }
    }
    // GET /api/tutors/earnings-report?from=2026-05-01&to=2026-05-31
    [HttpGet("earnings-report")]
    [Authorize(Policy = "TutorOnly")]
    public async Task<IActionResult> GetEarningsReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from > to)
            return BadRequest(new { message = "Başlangıç tarihi bitiş tarihinden büyük olamaz." });

        var result = await _tutorService.GetEarningsReportAsync(GetRequiredCurrentUserId(), from, to);
        return Ok(result);
    }
    // PUT /api/tutors/my-listings/{listingId}/photos/{photoId}/main
    [HttpPut("my-listings/{listingId:guid}/photos/{photoId:guid}/main")]
    [Authorize(Policy = "TutorOnly")]
    public async Task<IActionResult> SetMainPhoto(Guid listingId, Guid photoId)
    {
        var result = await _tutorService.SetMainListingPhotoAsync(
            GetRequiredCurrentUserId(), listingId, photoId);

        if (!result)
            return NotFound(new { message = "İlan veya fotoğraf bulunamadı." });

        return Ok(new { message = "Kapak fotoğrafı güncellendi." });
    }

    // PUT /api/tutors/my-listings/{listingId}/photos/sort-order
    [HttpPut("my-listings/{listingId:guid}/photos/sort-order")]
    [Authorize(Policy = "TutorOnly")]
    public async Task<IActionResult> UpdatePhotoSortOrder(
        Guid listingId,
        [FromBody] UpdateListingPhotoSortOrderDto request)
    {
        if (request is null || request.PhotoIds.Count == 0)
            return BadRequest(new { message = "Fotoğraf listesi boş olamaz." });

        var result = await _tutorService.UpdateListingPhotoSortOrderAsync(
            GetRequiredCurrentUserId(), listingId, request);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı." });

        return Ok(new { message = "Fotoğraf sırası güncellendi." });
    }

    // DELETE /api/tutors/my-listings/{listingId}/photos/{photoId}
    [HttpDelete("my-listings/{listingId:guid}/photos/{photoId:guid}")]
    [Authorize(Policy = "TutorOnly")]
    public async Task<IActionResult> DeletePhoto(Guid listingId, Guid photoId)
    {
        var result = await _tutorService.DeleteListingPhotoAsync(
            GetRequiredCurrentUserId(), listingId, photoId);

        if (!result)
            return NotFound(new { message = "İlan veya fotoğraf bulunamadı." });

        return Ok(new { message = "Fotoğraf silindi." });
    }
}