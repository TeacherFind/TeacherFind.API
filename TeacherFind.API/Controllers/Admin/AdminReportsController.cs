using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Reports;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public AdminReportsController(IReportService reportService)
        => _reportService = reportService;

    // GET /api/admin/reports — tüm şikayetleri listele
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _reportService.GetAllReportsAsync();
        return Ok(reports);
    }

    // PUT /api/admin/reports/{id} — şikayeti sonuçlandır
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveReportDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        try
        {
            var result = await _reportService.ResolveReportAsync(id, dto);
            if (!result)
                return NotFound(new { message = "Şikayet bulunamadı." });

            return Ok(new { message = "Şikayet sonuçlandırıldı." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}