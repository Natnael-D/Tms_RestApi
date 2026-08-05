// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Models;
// using TmsApi.Services;

// namespace TmsApi.Controllers;

// [ApiController]
// [Route("api/courses/{courseId:int}/enrollments")]
// public class EnrollmentsController : ControllerBase
// {
//     private readonly ICourseService _courseService;
//     private readonly IEnrollmentService _enrollmentService;

//     public EnrollmentsController(ICourseService courseService, IEnrollmentService enrollmentService)
//     {
//         _courseService = courseService;
//         _enrollmentService = enrollmentService;
//     }

//     // GET: /api/courses/{courseId}/enrollments - LIST all enrollments for a course
//     [HttpGet(Name = "ListCourseEnrollments")]
//     public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
//     {
//         // Check if course exists
//         var course = await _courseService.GetByIdAsync(courseId, ct);
//         if (course is null)
//             return NotFound();

//         var enrollments = await _enrollmentService.GetByCourseAsync(courseId, ct);
//         return Ok(enrollments);
//     }

//     // GET: /api/courses/{courseId}/enrollments/{id}
//     [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
//     public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
//     {
//         var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
//         if (enrollment is null)
//             return NotFound();

//         return Ok(enrollment);
//     }

//     // POST: /api/courses/{courseId}/enrollments
//     [HttpPost]
//     public async Task<IActionResult> EnrollStudent(int courseId, [FromBody] EnrollStudentRequest request, CancellationToken ct)
//     {
//         var course = await _courseService.GetByIdWithEnrollmentCountAsync(courseId, ct);
//         if (course is null)
//             return NotFound(new ProblemDetails
//             {
//                 Title = "Course not found",
//                 Detail = $"Course with ID {courseId} was not found.",
//                 Status = StatusCodes.Status404NotFound
//             });

//         if (await _courseService.IsCourseFullAsync(courseId, ct))
//         {
//             return Conflict(new ProblemDetails
//             {
//                 Title = "Course is full",
//                 Detail = $"Course '{course.Code}' has reached its maximum capacity of {course.MaxCapacity}.",
//                 Status = StatusCodes.Status409Conflict
//             });
//         }

//         var enrollment = await _enrollmentService.CreateAsync(courseId, request, ct);

//         return CreatedAtAction(
//             nameof(GetEnrollment), 
//             new { courseId, id = enrollment.Id }, 
//             enrollment);
//     }
// }

// the below code is enrollments controller with meta data

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Models;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(ICourseService courseService, IEnrollmentService enrollmentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet(Name = "ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrolments for a course")]
    [EndpointDescription("Returns all enrolments for a specific course. Returns 404 if the course does not exist.")]
    public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
    {
        var course = await _courseService.GetByIdAsync(courseId, ct);
        if (course is null)
            return NotFound();

        var enrollments = await _enrollmentService.GetByCourseAsync(courseId, ct);
        return Ok(enrollments);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrolment for a course")]
    [EndpointDescription("Returns a single enrolment by ID. Returns 404 if the enrolment does not exist.")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(courseId, id, ct);
        if (enrollment is null)
            return NotFound();

        return Ok(enrollment);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enrol a student in a course")]
    [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(int courseId, [FromBody] EnrollStudentRequest request, CancellationToken ct)
    {
        var course = await _courseService.GetByIdWithEnrollmentCountAsync(courseId, ct);
        if (course is null)
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail = $"Course with ID {courseId} was not found.",
                Status = StatusCodes.Status404NotFound
            });

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