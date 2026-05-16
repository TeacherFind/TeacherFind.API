using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Application.Features.Notifications;
using TeacherFind.Contracts.Listings;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    // TÜM İLANLAR + FİLTRE + PAGINATION
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ListingFilterRequestDto request)
    {
        var result = await _listingService.FilterAsync(request);
        return Ok(result);
    }

    // TEK İLAN
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var listing = await _listingService.GetByIdAsync(id);

        if (listing == null)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(listing);
    }

    // İLAN OLUŞTUR
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateListingRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "İstek boş olamaz" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz kullanıcı bilgisi" });

        await _listingService.CreateListingAsync(request, userId);

        return Ok(new { message = "İlan oluşturuldu" });
    }

    // İLAN GÜNCELLE
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListingRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "İstek boş olamaz" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz kullanıcı bilgisi" });

        var result = await _listingService.UpdateListingAsync(id, request, userId);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı veya işlem yetkiniz yok" });

        return Ok(new { message = "İlan güncellendi" });
    }

    // İLAN SİL
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Geçersiz kullanıcı bilgisi" });

        var result = await _listingService.DeleteListingAsync(id, userId);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı veya işlem yetkiniz yok" });

        return Ok(new { message = "İlan silindi" });
    }
}