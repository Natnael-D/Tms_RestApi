using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Models;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly LinkGenerator _linkGenerator;

    public CoursesController(ICourseService courseService, LinkGenerator linkGenerator)
    {
        _courseService = courseService;
        _linkGenerator = linkGenerator;
    }

    // GET: /api/courses (PAGINATED)
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }

    // GET: /api/courses/{id} (WITH HATEOAS LINKS)
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await _courseService.GetEnrollmentInfoAsync(id, ct);
        if (course is null)
            return NotFound();

        var links = new List<LinkDto>();

        // Self link
        var selfLink = _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
        if (selfLink is not null)
            links.Add(new LinkDto(selfLink, "self", "GET"));

        // Update link (PUT)
        var updateLink = _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
        if (updateLink is not null)
            links.Add(new LinkDto(updateLink, "update", "PUT"));

        // Delete link
        var deleteLink = _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
        if (deleteLink is not null)
            links.Add(new LinkDto(deleteLink, "delete", "DELETE"));

        // Enrollments list link
        var enrollmentsLink = _linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id });
        if (enrollmentsLink is not null)
            links.Add(new LinkDto(enrollmentsLink, "enrollments", "GET"));

        // Conditional: Enroll link (only if course is not full)
        if (course.EnrollmentCount < course.MaxCapacity)
        {
            var enrollLink = _linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id });
            if (enrollLink is not null)
                links.Add(new LinkDto(enrollLink, "enroll", "POST"));
        }

        var detailDto = new CourseDetailDto
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detailDto);
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
//the above code is for exercise 5 on module 6 session 3  CoursesController with HATEOAS

// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Routing;
// using TmsApi.Models;
// using TmsApi.Services;

// namespace TmsApi.Controllers;

// [ApiController]
// [Route("api/courses")]
// [Tags("Courses")]
// [Produces("application/json")]
// [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
// public class CoursesController : ControllerBase
// {
//     private readonly ICourseService _courseService;
//     private readonly LinkGenerator _linkGenerator;

//     public CoursesController(ICourseService courseService, LinkGenerator linkGenerator)
//     {
//         _courseService = courseService;
//         _linkGenerator = linkGenerator;
//     }

//     [HttpGet]
//     [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
//     [EndpointSummary("List courses with pagination")]
//     [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50.")]
//     public async Task<IActionResult> GetCourses([FromQuery] PagedRequest request, CancellationToken ct)
//     {
//         var result = await _courseService.GetCoursesAsync(request, ct);
//         return Ok(result);
//     }

//     [HttpGet("{id:int}", Name = nameof(GetCourseById))]
//     [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
//     [EndpointSummary("Get a course by ID")]
//     [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
//     public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
//     {
//         var course = await _courseService.GetEnrollmentInfoAsync(id, ct);
//         if (course is null)
//             return NotFound();

//         var links = new List<LinkDto>();

//         var selfLink = _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
//         if (selfLink is not null)
//             links.Add(new LinkDto(selfLink, "self", "GET"));

//         var updateLink = _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
//         if (updateLink is not null)
//             links.Add(new LinkDto(updateLink, "update", "PUT"));

//         var deleteLink = _linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id });
//         if (deleteLink is not null)
//             links.Add(new LinkDto(deleteLink, "delete", "DELETE"));

//         var enrollmentsLink = _linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id });
//         if (enrollmentsLink is not null)
//             links.Add(new LinkDto(enrollmentsLink, "enrollments", "GET"));

//         if (course.EnrollmentCount < course.MaxCapacity)
//         {
//             var enrollLink = _linkGenerator.GetPathByName(HttpContext, "ListCourseEnrollments", new { courseId = id });
//             if (enrollLink is not null)
//                 links.Add(new LinkDto(enrollLink, "enroll", "POST"));
//         }

//         var detailDto = new CourseDetailDto
//         {
//             Id = course.Id,
//             Code = course.Code,
//             Title = course.Title,
//             MaxCapacity = course.MaxCapacity,
//             EnrollmentCount = course.EnrollmentCount,
//             Links = links
//         };

//         return Ok(detailDto);
//     }

//     [HttpPost]
//     [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
//     [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
//     [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
//     [EndpointSummary("Create a new course")]
//     [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
//     public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken ct)
//     {
//         if (await _courseService.CodeExistsAsync(request.Code, ct))
//         {
//             return Conflict(new ProblemDetails
//             {
//                 Title = "Course code already exists",
//                 Detail = $"A course with code '{request.Code}' is already registered.",
//                 Status = StatusCodes.Status409Conflict
//             });
//         }

//         var result = await _courseService.CreateAsync(request, ct);
        
//         return CreatedAtAction(
//             nameof(GetCourseById), 
//             new { id = result.Id }, 
//             result);
//     }
// }