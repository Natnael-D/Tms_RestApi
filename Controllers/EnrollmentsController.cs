using Microsoft.AspNetCore.Mvc;
using TmsApi.Models;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    // GET: /api/courses/{courseId}/enrollments/{id}
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
        if (enrollment is null)
            return NotFound();

        return Ok(enrollment);
    }

    // POST: /api/courses/{courseId}/enrollments
    [HttpPost]
    public async Task<IActionResult> EnrollStudent(int courseId, [FromBody] EnrollStudentRequest request, CancellationToken ct)
    {
        // Check if course exists (404 if not found)
        var course = await _courseService.GetByIdWithEnrollmentCountAsync(courseId, ct);
        if (course is null)
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail = $"Course with ID {courseId} was not found.",
                Status = StatusCodes.Status404NotFound
            });

        // Check if course is full (409 Conflict)
        if (await _courseService.IsCourseFullAsync(courseId, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Code}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var enrollment = await _enrollmentService.CreateAsync(courseId, request, ct);

        return CreatedAtAction(
            nameof(GetEnrollment), 
            new { courseId, id = enrollment.Id }, 
            enrollment);
    }
}