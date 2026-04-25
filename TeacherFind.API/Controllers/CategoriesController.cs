using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetAll()
    {
        var subjects = await _context.Subjects
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var grouped = subjects
            .GroupBy(x => x.Category)
            .Select(g => new
            {
                Category = g.Key,
                Subjects = g.Select(s => new { s.Id, s.Name }).ToList()
            })
            .ToList();

        return Ok(grouped);
    }
}