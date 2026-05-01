using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/education")]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;

    public EducationController(IEducationService educationService)
    {
        _educationService = educationService;
    }

    // GET /api/education/universities
    [HttpGet("universities")]
    public async Task<IActionResult> GetUniversities()
    {
        var result = await _educationService.GetUniversitiesAsync();

        return Ok(result);
    }

    // GET /api/education/departments?universityId={id}
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] Guid? universityId)
    {
        var result = await _educationService.GetDepartmentsAsync(universityId);

        return Ok(result);
    }
}