using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminReviewsController(AppDbContext context) => _context = context;

    // GET /api/admin/reviews — tüm yorumları listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Reviewer)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.ListingId,
                ReviewerId = r.UserId,
                ReviewerName = r.Reviewer != null ? r.Reviewer.FullName : "Bilinmiyor",
                r.Rating,
                r.Comment,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }

    // DELETE /api/admin/reviews/{id} — yorum sil
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
            return NotFound(new { message = "Yorum bulunamadı." });

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Yorum silindi." });
    }
}