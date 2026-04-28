using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Favorites;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
        => _favoriteService = favoriteService;

    // GET /api/favorites
    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var result = await _favoriteService.GetFavoritesAsync(GetUserId());
        return Ok(result);
    }

    // Task 7 — POST /api/favorites/toggle  (always 200, never 400)
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] ToggleFavoriteRequestDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        var added = await _favoriteService.ToggleFavoriteAsync(GetUserId(), dto.TutorId);

        return Ok(new ToggleFavoriteResponseDto
        {
            IsFavorite = added,
            Message = added ? "Favorilere eklendi" : "Favorilerden çıkarıldı"
        });
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}