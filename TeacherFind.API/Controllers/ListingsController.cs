using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Common;
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

    //  TÜM İLANLAR
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var listings = await _listingService.GetListingsAsync();
        return Ok(listings);
    }

    //  TEK İLAN
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var listing = await _listingService.GetByIdAsync(id);

        if (listing == null)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(listing);
    }

    //  İLAN OLUŞTUR
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateListingRequestDto request)
    {
        if (request == null)
            return BadRequest(new { message = "İstek boş olamaz" });

        await _listingService.CreateListingAsync(request);

        return Ok(new { message = "İlan oluşturuldu" });
    }

    //  FİLTRE
    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] ListingFilterRequestDto filter)
    {
        var result = await _listingService.FilterAsync(filter);
        return Ok(result);
    }

    // İLAN GÜNCELLE
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListingRequestDto request)
    {
        var result = await _listingService.UpdateListingAsync(id, request);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(new { message = "İlan güncellendi" });
    }

    // İLAN SİL
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _listingService.DeleteListingAsync(id);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(new { message = "İlan silindi" });
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequestDto request)
    {
        var result = await _listingService.GetPagedAsync(request);
        return Ok(result);
    }
}