using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Listings;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // ⭐ YORUM EKLE
    [HttpPost("{listingId}")]
    [Authorize]
    public async Task<IActionResult> Add(Guid listingId, [FromBody] ReviewRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _reviewService.AddReviewAsync(userId, listingId, request.Rating, request.Comment);

        return Ok(new { message = "Yorum eklendi" });
    }

    // 📄 YORUMLARI GETİR
    [HttpGet("{listingId}")]
    public async Task<IActionResult> Get(Guid listingId)
    {
        var reviews = await _reviewService.GetReviewsAsync(listingId);
        return Ok(reviews);
    }

    // ⭐ ORTALAMA PUAN
    [HttpGet("{listingId}/average")]
    public async Task<IActionResult> GetAverage(Guid listingId)
    {
        var avg = await _reviewService.GetAverageRatingAsync(listingId);
        return Ok(avg);
    }
}