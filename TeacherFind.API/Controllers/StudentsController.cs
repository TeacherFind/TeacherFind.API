using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
        => _studentService = studentService;

    // GET /api/students/lessons
    [HttpGet("lessons")]
    public async Task<IActionResult> GetLessonHistory()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _studentService.GetLessonHistoryAsync(userId);
        return Ok();
    }
}