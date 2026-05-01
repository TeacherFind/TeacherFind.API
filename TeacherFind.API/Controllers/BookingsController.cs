using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Bookings;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST /api/bookings
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto request)
    {
        if (request is null)
            return BadRequest(new { message = "Rezervasyon bilgileri gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _bookingService.CreateAsync(currentUserId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/bookings/my
    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var result = await _bookingService.GetMyBookingsAsync(currentUserId);

        return Ok(result);
    }

    // GET /api/bookings/occupied?teacherListingId=...&from=2026-05-01&to=2026-05-08
    [AllowAnonymous]
    [HttpGet("occupied")]
    public async Task<IActionResult> GetOccupiedSlots(
        [FromQuery] Guid teacherListingId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (teacherListingId == Guid.Empty)
            return BadRequest(new { message = "Öğretmen ilan ID bilgisi zorunludur." });

        if (from == default || to == default)
            return BadRequest(new { message = "Başlangıç ve bitiş tarihi zorunludur." });

        try
        {
            var result = await _bookingService.GetOccupiedSlotsAsync(
                teacherListingId,
                from,
                to);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/bookings/{id}/cancel
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] CancelBookingRequestDto request)
    {
        if (request is null)
            return BadRequest(new { message = "İptal bilgisi gönderilmedi." });

        var currentUserId = GetRequiredCurrentUserId();

        try
        {
            var result = await _bookingService.CancelAsync(
                id,
                currentUserId,
                request);

            if (!result)
                return NotFound(new { message = "Rezervasyon bulunamadı veya erişim yetkiniz yok." });

            return Ok(new { message = "Rezervasyon iptal edildi." });
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
}