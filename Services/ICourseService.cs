using TmsApi.Models;

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool> IsCourseFullAsync(int id, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdWithEnrollmentCountAsync(int id, CancellationToken ct);
}