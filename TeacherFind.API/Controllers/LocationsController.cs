using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Contracts.Locations;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LocationsController(AppDbContext context) => _context = context;

    // GET /api/locations/cities
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _context.Cities
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CityDto { Id = x.Id, PlateCode = x.PlateCode, Name = x.Name })
            .ToListAsync();

        return Ok(cities);
    }

    // GET /api/locations/districts?cityId={cityId}
    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] Guid cityId)
    {
        var districts = await _context.Districts
            .Where(x => x.CityId == cityId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new DistrictDto { Id = x.Id, Code = x.Code, Name = x.Name })
            .ToListAsync();

        return Ok(districts);
    }

    // GET /api/locations/neighborhoods?districtId={districtId}
    [HttpGet("neighborhoods")]
    public async Task<IActionResult> GetNeighborhoods([FromQuery] Guid districtId)
    {
        var neighborhoods = await _context.Neighborhoods
            .Where(x => x.DistrictId == districtId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new NeighborhoodDto { Id = x.Id, Code = x.Code, Name = x.Name })
            .ToListAsync();

        return Ok(neighborhoods);
    }
}