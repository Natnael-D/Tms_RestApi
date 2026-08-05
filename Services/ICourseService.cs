using TmsApi.Models;

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool> IsCourseFullAsync(int id, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdWithEnrollmentCountAsync(int id, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    
    // NEW: For HATEOAS - get course with enrollment count only (lighter)
    Task<CourseEnrollmentInfo?> GetEnrollmentInfoAsync(int id, CancellationToken ct);
}

// NEW DTO for enrollment info
public class CourseEnrollmentInfo
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int EnrollmentCount { get; set; }
}