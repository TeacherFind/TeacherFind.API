using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Listings;
using TeacherFind.Contracts.Reviews;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // GET /api/reviews/{listingId}
    [HttpGet("{listingId:guid}")]
    public async Task<IActionResult> Get(Guid listingId)
    {
        var reviews = await _reviewService.GetReviewsAsync(listingId);

        return Ok(reviews);
    }

    // GET /api/reviews/{listingId}/average
    [HttpGet("{listingId:guid}/average")]
    public async Task<IActionResult> GetAverage(Guid listingId)
    {
        var averageRating = await _reviewService.GetAverageRatingAsync(listingId);

        return Ok(averageRating);
    }

    // POST /api/reviews
    // Booking tabanlı güvenli yorum endpoint'i.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequestDto request)
    {
        if (request is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Geçersiz token." });

        try
        {
            await _reviewService.CreateReviewAsync(userId, request);

            return Ok(new { message = "Yorumunuz başarıyla eklendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/reviews/{listingId}
    // Eski endpoint kapatıldı. BookingId tabanlı POST /api/reviews kullanılmalı.
    [HttpPost("{listingId:guid}")]
    [Authorize]
    public async Task<IActionResult> Add(Guid listingId, [FromBody] ReviewRequest request)
    {
        if (request is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(new { message = "Puan 1 ile 5 arasında olmalıdır." });

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _reviewService.AddReviewAsync(userId, listingId, request.Rating, request.Comment);

        return Ok(new { message = "Yorum eklendi." });
    }
}   