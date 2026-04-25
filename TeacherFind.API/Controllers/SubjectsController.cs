using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SubjectsController(AppDbContext context) => _context = context;

    // GET /api/subjects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var subjects = await _context.Subjects
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Category })
            .ToListAsync();

        return Ok(subjects);
    }
}