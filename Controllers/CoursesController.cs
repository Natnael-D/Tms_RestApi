using Microsoft.AspNetCore.Mvc;
using TmsApi.Models;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET: /api/courses (PAGINATED - NEW!)
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    // GET: /api/courses/{id}
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound();

        return Ok(course);
    }

    // POST: /api/courses
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        if (await _courseService.CodeExistsAsync(request.Code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A course with code '{request.Code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var result = await _courseService.CreateAsync(request, ct);
        
        return CreatedAtAction(
            nameof(GetCourseById), 
            new { id = result.Id }, 
            result);
    }
}