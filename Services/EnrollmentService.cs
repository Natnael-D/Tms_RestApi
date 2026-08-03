using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;

namespace TmsApi.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                EnrolledAt = e.EnrolledAt
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentResponseDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                StudentId = e.StudentId,
                EnrolledAt = e.EnrolledAt
            })
            .ToListAsync(ct);
    }

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Enrolled student {StudentId} in course {CourseId}", request.StudentId, courseId);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }
}