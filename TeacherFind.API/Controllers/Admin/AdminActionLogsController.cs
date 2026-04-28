using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/action-logs")]
[Authorize(Policy = "AdminOnly")]
public class AdminActionLogsController : ControllerBase
{
    private readonly IAdminActionLogService _adminActionLogService;

    public AdminActionLogsController(IAdminActionLogService adminActionLogService)
    {
        _adminActionLogService = adminActionLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AdminActionLogQuery query)
    {
        var result = await _adminActionLogService.GetLogsAsync(query);
        return Ok(result);
    }
}