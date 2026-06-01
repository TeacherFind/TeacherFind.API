using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context) => _context = context;

    // GET /api/categories
    [HttpGet]
    [EnableRateLimiting("PublicListPolicy")]
    public async Task<IActionResult> GetAll()
    {
        var subjects = await _context.Subjects
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var grouped = subjects
            .GroupBy(x => x.Category)
            .Select(g => new
            {
                Category = g.Key,
                Subjects = g.Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.Name,
                    s.Level
                }).ToList()
            })
            .ToList();

        return Ok(grouped);
    }
}