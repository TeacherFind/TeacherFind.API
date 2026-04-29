using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Reports;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
        => _reportService = reportService;

    // POST /api/reports  — öğrenci şikayet oluşturur
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        var reporterId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        try
        {
            await _reportService.CreateReportAsync(reporterId, dto);
            return Ok(new { message = "Şikayetiniz alındı. En kısa sürede incelenecektir." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
