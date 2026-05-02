using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Students;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // GET /api/students/dashboard-stats
    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _studentService.GetDashboardStatsAsync(currentUserId);

        return Ok(result);
    }

    // GET /api/students/lessons
    [HttpGet("lessons")]
    public async Task<IActionResult> GetLessonHistory()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _studentService.GetLessonHistoryAsync(currentUserId);

        return Ok(result);
    }

    // GET /api/students/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _studentService.GetMyProfileAsync(currentUserId);

        if (result is null)
            return NotFound(new { message = "Öğrenci profili bulunamadı." });

        return Ok(result);
    }

    // PUT /api/students/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileDto request)
    {
        if (request is null)
            return BadRequest(new { message = "Profil bilgileri gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        var success = await _studentService.UpdateMyProfileAsync(currentUserId, request);

        if (!success)
            return NotFound(new { message = "Öğrenci profili bulunamadı." });

        return Ok(new { message = "Profil başarıyla güncellendi." });
    }

    // POST /api/students/avatar
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile? file)
    {
        var currentUserId = GetRequiredCurrentUserId();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Dosya gönderilmedi." });

        try
        {
            var profileImageUrl = await SaveAvatarFileAsync(currentUserId, file);

            var success = await _studentService.UpdateAvatarAsync(
                currentUserId,
                profileImageUrl);

            if (!success)
                return NotFound(new { message = "Öğrenci bulunamadı." });

            return Ok(new
            {
                message = "Avatar başarıyla yüklendi.",
                profileImageUrl
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetRequiredCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Geçersiz kullanıcı tokenı.");

        return userId;
    }

    private static async Task<string> SaveAvatarFileAsync(
        Guid currentUserId,
        IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Sadece .jpg, .jpeg, .png veya .webp dosyaları yüklenebilir.");

        const long maxFileSize = 2 * 1024 * 1024;

        if (file.Length > maxFileSize)
            throw new InvalidOperationException("Dosya boyutu en fazla 2 MB olabilir.");

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "avatars");

        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"student_{currentUserId}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/avatars/{fileName}";
    }
}