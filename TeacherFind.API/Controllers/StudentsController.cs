using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Students;

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

    // GET /api/students/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Geçersiz token." });

        var result = await _studentService.GetMyProfileAsync(userId);

        if (result is null)
            return NotFound(new { message = "Öğrenci profili bulunamadı." });

        return Ok(result);
    }

    // PUT /api/students/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateStudentProfileDto request)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "Geçersiz token." });

        var success = await _studentService.UpdateMyProfileAsync(userId, request);

        if (!success)
            return NotFound(new { message = "Öğrenci profili bulunamadı." });

        return Ok(new { message = "Profil başarıyla güncellendi." });
    }
}