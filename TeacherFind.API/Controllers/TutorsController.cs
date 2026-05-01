using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Bookings;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Application.Abstractions.Repositories;

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

    // PUT /api/tutors/profile
    [Authorize(Policy = "TutorOnly")]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTutorProfileDto request)
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.UpdateMyProfileAsync(currentUserId, request);

        if (!result)
            return BadRequest(new { message = "Profil güncellenemedi." });

        return Ok(new { message = "Profil başarıyla güncellendi." });
    }

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
        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _bookingService.RejectAsync(
                id,
                currentUserId,
                request);

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

    // GET /api/tutors/my-students
    [Authorize(Policy = "TutorOnly")]
    [HttpGet("my-students")]
    public async Task<IActionResult> GetMyStudents()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _tutorService.GetMyStudentsAsync(currentUserId);

        return Ok(result);
    }

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

    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized(new { message = "Kullanıcı doğrulanamadı." });

        var profile = await _tutorService.GetMyProfileAsync(userId.Value);

        if (profile is null)
            return NotFound(new { message = "Öğretmen profili bulunamadı." });

        return Ok(profile);
    }

    // POST /api/tutors/avatar
    [Authorize(Policy = "TutorOnly")]
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
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

        if (!Directory.Exists(uploadsFolder))
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

        string? fileUrl = null;
        string? fileName = null;
        string? contentType = null;

        if (file is not null && file.Length > 0)
        {
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Sadece PDF veya görsel dosyası yüklenebilir." });

            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
                return BadRequest(new { message = "Dosya boyutu en fazla 5 MB olabilir." });

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "certificates");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var storedFileName = $"{currentUserId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            fileUrl = $"/uploads/certificates/{storedFileName}";
            fileName = file.FileName;
            contentType = file.ContentType;
        }

        try
        {
            var result = await _tutorService.AddMyCertificateAsync(
                currentUserId,
                new AddTutorCertificateDto
                {
                    Name = name,
                    Organization = organization,
                    Year = year,
                    FileUrl = fileUrl,
                    FileName = fileName,
                    ContentType = contentType
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

        var result = await _tutorService.DeleteMyCertificateAsync(
            currentUserId,
            id);

        if (!result)
            return NotFound(new { message = "Sertifika bulunamadı veya erişim yetkiniz yok." });

        return Ok(new { message = "Sertifika silindi." });
    }
}