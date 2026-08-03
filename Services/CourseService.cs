using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Models;

namespace TmsApi.Services;

public class CourseService : ICourseService
{
    private readonly TmsDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(TmsDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                MaxCapacity = c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto?> GetByIdWithEnrollmentCountAsync(int id, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                MaxCapacity = c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);

        return (await GetByIdAsync(course.Id, ct))!;
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, ct);
    }

    public async Task<bool> IsCourseFullAsync(int id, CancellationToken ct)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
            return true;

        return course.Enrollments.Count >= course.MaxCapacity;
    }
}