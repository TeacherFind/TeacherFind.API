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
            .Where(x => x.IsActive)
            .OrderBy(x => x.Stage)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Stage,
                x.Category,
                x.Name,
                x.Level,
                x.IsActive
            })
            .ToListAsync();

        return Ok(subjects);
    }

    // GET /api/subjects/grouped — Stage > Category > Subjects hierarchy
    [HttpGet("grouped")]
    public async Task<IActionResult> GetGrouped()
    {
        var subjects = await _context.Subjects
            .Where(x => x.IsActive)
            .OrderBy(x => x.Stage)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync();

        var grouped = subjects
            .GroupBy(x => x.Stage)
            .Select(stageGroup => new
            {
                Stage = stageGroup.Key,
                Categories = stageGroup
                    .GroupBy(x => x.Category)
                    .Select(catGroup => new
                    {
                        Category = catGroup.Key,
                        Subjects = catGroup.Select(s => new
                        {
                            s.Id,
                            s.Code,
                            s.Name,
                            s.Level
                        }).ToList()
                    }).ToList()
            }).ToList();

        return Ok(grouped);
    }
}