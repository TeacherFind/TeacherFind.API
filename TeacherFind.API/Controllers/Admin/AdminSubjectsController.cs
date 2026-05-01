using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/subjects")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminSubjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminSubjectsController(AppDbContext context) => _context = context;

    // GET /api/admin/subjects
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var subjects = await _context.Subjects
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.Category })
            .ToListAsync();

        return Ok(subjects);
    }

    // POST /api/admin/subjects
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Category))
            return BadRequest(new { message = "Konu adı ve kategori zorunludur." });

        var exists = await _context.Subjects
            .AnyAsync(x => x.Name == dto.Name.Trim() && x.Category == dto.Category.Trim());

        if (exists)
            return BadRequest(new { message = "Bu konu bu kategori altında zaten mevcut." });

        var subject = new Subject
        {
            Name = dto.Name.Trim(),
            Category = dto.Category.Trim()
        };

        await _context.Subjects.AddAsync(subject);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Konu oluşturuldu.", subject.Id });
    }

    // PUT /api/admin/subjects/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubjectDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id);
        if (subject is null)
            return NotFound(new { message = "Konu bulunamadı." });

        subject.Name = dto.Name.Trim();
        subject.Category = dto.Category.Trim();

        await _context.SaveChangesAsync();
        return Ok(new { message = "Konu güncellendi." });
    }

    // DELETE /api/admin/subjects/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id);
        if (subject is null)
            return NotFound(new { message = "Konu bulunamadı." });

        // İlana bağlıysa silme
        var hasListings = await _context.TeacherListings.AnyAsync(x => x.SubjectId == id);
        if (hasListings)
            return BadRequest(new { message = "Bu konuya ait aktif ilanlar var. Önce ilanları silin." });

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Konu silindi." });
    }
}