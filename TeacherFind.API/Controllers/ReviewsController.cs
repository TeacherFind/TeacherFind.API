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
        => _reviewService = reviewService;

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
        var avg = await _reviewService.GetAverageRatingAsync(listingId);
        return Ok(avg);
    }

    // POST /api/reviews — booking tabanlı, güvenli yorum
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequestDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        try
        {
            await _reviewService.CreateReviewAsync(userId, dto);
            return Ok(new { message = "Yorumunuz başarıyla eklendi." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/reviews/{listingId} — eski endpoint, geriye dönük uyumluluk için korundu
    [HttpPost("{listingId:guid}")]
    [Authorize]
    public async Task<IActionResult> Add(Guid listingId, [FromBody] ReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _reviewService.AddReviewAsync(userId, listingId, request.Rating, request.Comment);
        return Ok(new { message = "Yorum eklendi." });
    }
}