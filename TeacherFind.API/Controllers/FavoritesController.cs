using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Application.Features.Favorites;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    // ❤️ EKLE / ❌ ÇIKAR
    [HttpPost("{listingId}")]
    public async Task<IActionResult> Toggle(Guid listingId)
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        await _favoriteService.ToggleFavoriteAsync(userId, listingId);

        return Ok(new { message = "Favori güncellendi" });
    }

    // 📄 FAVORİ LİSTESİ
    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = Guid.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );

        var result = await _favoriteService.GetFavoritesAsync(userId);

        return Ok(result);
    }
}