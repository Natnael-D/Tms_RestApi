using System.ComponentModel.DataAnnotations;

namespace TmsApi.Models;

// Request DTO for creating a course
public class CreateCourseRequest
{
    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{3}$", 
        ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    public required string Code { get; init; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Range(1, 200)]
    public int MaxCapacity { get; init; } = 30;
}

// Request DTO for updating a course
public class UpdateCourseRequest
{
    [Required]
    [RegularExpression(@"^[A-Z]{3}-\d{3}$", 
        ErrorMessage = "Code must follow the pattern XXX-000 (e.g., CSE-101).")]
    public required string Code { get; init; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Range(1, 200)]
    public int MaxCapacity { get; init; } = 30;
}

// Response DTO for course
public class CourseResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int EnrollmentCount { get; set; }
}

// Request DTO for enrolling a student
public class EnrollStudentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public required int StudentId { get; init; }
}

// Response DTO for enrollment
public class EnrollmentResponseDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public DateTime EnrolledAt { get; set; }
}

public record CourseDetailDto
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required int MaxCapacity { get; init; }
    public required int EnrollmentCount { get; init; }
    public required IReadOnlyList<LinkDto> Links { get; init; }
}
