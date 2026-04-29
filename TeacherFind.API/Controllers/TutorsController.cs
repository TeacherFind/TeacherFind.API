using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Tutors;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/tutors")]
public class TutorsController : ControllerBase
{
    private readonly ITutorService _tutorService;

    public TutorsController(ITutorService tutorService)
    {
        _tutorService = tutorService;
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

        var result = await _tutorService.UpdateMyListingAsync(
            currentUserId,
            id,
            request);

        if (result is null)
            return NotFound(new { message = "İlan bulunamadı veya bu ilana erişim yetkiniz yok." });

        return Ok(result);
    }
}