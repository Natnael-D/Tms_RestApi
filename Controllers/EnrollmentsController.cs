using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // Required for .ToListAsync(), .FindAsync(), etc.
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly TmsDbContext _context;

    public EnrollmentsController(TmsDbContext context)
    {
        _context = context;
    }

    // GET: api/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var enrollments = await _context.Enrollments.ToListAsync();
        return Ok(enrollments);
    }

    // GET: api/enrollments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment is null)
            return NotFound();
        return Ok(enrollment);
    }

    // POST: api/enrollments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrolledAt = DateTime.UtcNow
        };

        await _context.Enrollments.AddAsync(enrollment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = enrollment.Id }, enrollment);
    }

    // DELETE: api/enrollments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment is null)
            return NotFound();

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record CreateEnrollmentRequest(int StudentId, int CourseId);