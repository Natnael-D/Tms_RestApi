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

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        // Step 1: Start with a no-tracking IQueryable
        var query = _context.Courses.AsNoTracking();

        // Step 2: Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{search}%") ||
                EF.Functions.ILike(c.Code, $"%{search}%"));
        }

        // Step 3: Count BEFORE paging (this gives the total count)
        var totalCount = await query.CountAsync(ct);

        // Step 4: Apply ordering
        query = request.OrderBy.ToLower() switch
        {
            "code" => request.Descending 
                ? query.OrderByDescending(c => c.Code) 
                : query.OrderBy(c => c.Code),
            "maxcapacity" => request.Descending 
                ? query.OrderByDescending(c => c.MaxCapacity) 
                : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending 
                ? query.OrderByDescending(c => c.Title) 
                : query.OrderBy(c => c.Title)
        };

        // Step 5: Apply Skip/Take for pagination
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourseResponseDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                MaxCapacity = c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync(ct);

        // Step 6: Return the paged response
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CourseEnrollmentInfo?> GetEnrollmentInfoAsync(int id, CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseEnrollmentInfo
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                MaxCapacity = c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .FirstOrDefaultAsync(ct);
    }
}
