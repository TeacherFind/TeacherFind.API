using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminCategoriesController(AppDbContext context) => _context = context;

    // GET /api/admin/categories — distinct kategori listesi + konu sayısı
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _context.Subjects
            .GroupBy(x => x.Category)
            .Select(g => new
            {
                Name = g.Key,
                SubjectCount = g.Count()
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(categories);
    }

    // DELETE /api/admin/categories/{name} — kategorideki tüm konuları sil
    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(string name)
    {
        var subjects = await _context.Subjects
            .Where(x => x.Category == name)
            .ToListAsync();

        if (!subjects.Any())
            return NotFound(new { message = "Kategori bulunamadı." });

        // Herhangi bir konuya bağlı ilan var mı kontrol et
        var subjectIds = subjects.Select(s => s.Id).ToList();
        var hasListings = await _context.TeacherListings
            .AnyAsync(x => x.SubjectId != null && subjectIds.Contains(x.SubjectId.Value));

        if (hasListings)
            return BadRequest(new { message = "Bu kategoriye ait aktif ilanlar var. Önce ilanları silin." });

        _context.Subjects.RemoveRange(subjects);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"'{name}' kategorisi ve tüm konuları silindi." });
    }
}