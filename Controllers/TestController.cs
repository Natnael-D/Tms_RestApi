using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // ← Add this!
using TmsApi.Data;
using TmsApi.Entities;  
namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly TmsDbContext _context;

    public TestController(TmsDbContext context)
    {
        _context = context;
    }

    // ============================================
    // EXERCISE 2 Step 3: Deferred Execution
    // ============================================

    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (no database contact)...");
        var query = _context.Students.Where(s => s.GPA >= 3.0m);

        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);

        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList();

        Console.WriteLine(">>> STEP 4: Materialization finished. List populated.\n");
        return Ok(results);
    }

    // ============================================
    // EXERCISE 2 Step 4: SQL Translation Failure
    // ============================================

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = _context.Students
                .Where(s => IsHonorRoll(s.GPA))
                .ToList();
            return Ok(students);
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
            return BadRequest(new { Message = ex.Message });
        }
    }

    // ============================================
    // EXERCISE 2 Step 5: Registrar's Business Queries
    // ============================================

    [HttpGet("active-honors-count")]
    public async Task<IActionResult> GetActiveHonorsCount()
    {
        var count = await _context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        Console.WriteLine($"Active students with GPA >= 3.0: {count}");
        return Ok(new { Count = count });
    }

    [HttpGet("course-enrollment-counts")]
    public async Task<IActionResult> GetCourseEnrollmentCounts()
    {
        var results = await _context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> GetAverageGpaPerCourse()
    {
        var results = await _context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("zero-enrollments-subquery")]
    public async Task<IActionResult> GetZeroEnrollmentsSubquery()
    {
        var students = await _context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(students);
    }

    [HttpGet("zero-enrollments-leftjoin")]
    public async Task<IActionResult> GetZeroEnrollmentsLeftJoin()
    {
        var students = await _context.Students
            .GroupJoin(
                _context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, e) => new { Student = s, Enrollments = e })
            .Where(x => !x.Enrollments.Any())
            .Select(x => x.Student.Name)
            .ToListAsync();

        return Ok(students);
    }

    // ============================================
    // EXERCISE 3: Pagination and Aggregates
    // ============================================

    [HttpGet("students-paged")]
    public async Task<IActionResult> GetStudentsPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        Console.WriteLine($"\n>>> PAGINATION: Page {page}, PageSize {pageSize}");

        var students = await _context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Students.CountAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Students = students
        });
    }

    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses()
    {
        Console.WriteLine("\n>>> TOP 5 COURSES BY ENROLLMENT");

        var topCourses = await _context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync();

        return Ok(topCourses);
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        Console.WriteLine("\n>>> DASHBOARD SUMMARY");

        var totalStudents = await _context.Students.CountAsync();
        var totalCourses = await _context.Courses.CountAsync();
        var totalEnrollments = await _context.Enrollments.CountAsync();
        var activeStudents = await _context.Students.CountAsync(s => s.IsActive);
        var averageGPA = await _context.Students.AverageAsync(s => s.GPA);

        return Ok(new
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            TotalCourses = totalCourses,
            TotalEnrollments = totalEnrollments,
            AverageGPA = averageGPA
        });
    }



    // ============================================
    // EXERCISE 7: N+1 Query Fix
    // ============================================

    [HttpGet("n-plus-one-bad")]
    public async Task<IActionResult> TestNPlusOneBad()
    {
        Console.WriteLine("\n>>> N+1 BAD: Loading students with separate queries...");

        var students = await _context.Students
            .AsNoTracking()
            .ToListAsync();

        var results = new List<object>();
        foreach (var s in students)
        {
            // This creates a separate query for EACH student!
            var count = await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.StudentId == s.Id);

            results.Add(new { s.Name, EnrollmentCount = count });
            Console.WriteLine($"{s.Name}: {count} enrollments (separate query)");
        }

        return Ok(results);
    }

    [HttpGet("n-plus-one-fixed")]
    public async Task<IActionResult> TestNPlusOneFixed()
    {
        Console.WriteLine("\n>>> N+1 FIXED: Single query with projection...");

        // Single query with subquery - ONE round trip to the database!
        var results = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync();

        foreach (var r in results)
        {
            Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments (single query)");
        }

        return Ok(results);
    }

    [HttpGet("n-plus-one-include")]
    public async Task<IActionResult> TestNPlusOneInclude()
    {
        Console.WriteLine("\n>>> N+1 WITH INCLUDE: Loads all enrollments in one query...");

        var students = await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
            .ToListAsync();

        var results = new List<object>();
        foreach (var s in students)
        {
            results.Add(new { s.Name, EnrollmentCount = s.Enrollments.Count });
            Console.WriteLine($"{s.Name}: {s.Enrollments.Count} enrollments (include)");
        }

        return Ok(results);
    }
    // ============================================
    // CONCURRENCY TEST
    // ============================================

    [HttpGet("student/{id}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _context.Students
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.GPA,
                s.IsActive,
                s.IsDeleted,
                s.Version,
                LastUpdated = EF.Property<DateTime>(s, "LastUpdated")
            })
            .FirstOrDefaultAsync();

        if (student is null)
            return NotFound();

        return Ok(student);
    }

    [HttpPut("student/{id}")]
    public async Task<IActionResult> UpdateStudent(
        int id, 
        [FromBody] UpdateStudentRequest request)
    {
         var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (student is null)
            return NotFound();

        Console.WriteLine($"\n>>> CONCURRENCY TEST: Updating student {id}");
        Console.WriteLine($"   Current DB Version: {student.Version}, Request Version: {request.Version}");

        // Use the client-supplied Version as the original value for concurrency checking
        _context.Entry(student).Property(nameof(Student.Version)).OriginalValue = request.Version;

        student.Name = request.Name;
        student.GPA = request.GPA;
        
        _context.Entry(student).Property("LastUpdated").CurrentValue = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"   ✅ Update successful - New Version: {student.Version}");
            
            return Ok(new 
            { 
                Message = "Updated successfully", 
                student.Name, 
                student.GPA,
                Version = student.Version,
                LastUpdated = _context.Entry(student).Property("LastUpdated").CurrentValue
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"   ❌ Concurrency conflict detected!");
            
            await _context.Entry(student).ReloadAsync();
            
            return Conflict(new 
            { 
                Message = "Concurrency conflict - record was modified by another user",
                CurrentName = student.Name,
                CurrentGPA = student.GPA,
                CurrentVersion = student.Version,
                Error = ex.Message 
            });
        }
    }
    public class UpdateStudentRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal GPA { get; set; }
    public uint Version { get; set; }
}


    // ============================================
    // EXERCISE 9: Bulk Archive and Soft Delete
    // ============================================

    [HttpPost("enrollments/archive")]
    public async Task<IActionResult> ArchiveOldEnrollments([FromQuery] int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

        Console.WriteLine($"\n>>> BULK ARCHIVE: Archiving enrollments older than {daysOld} days");
        Console.WriteLine($"Cutoff date: {cutoffDate}");

        // Single bulk UPDATE - no loading into memory!
        var count = await _context.Enrollments
            .Where(e => e.EnrolledAt < cutoffDate && !e.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsArchived, true));

        Console.WriteLine($"Archived {count} enrollments in a single batch");


        return Ok(new
        {
            ArchivedCount = count,
            CutoffDate = cutoffDate,
            Message = $"Archived {count} enrollments older than {daysOld} days"
        });
    }

    [HttpPost("student/soft-delete/{id}")]
    public async Task<IActionResult> SoftDeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null)
            return NotFound();

        student.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Student {student.Name} soft-deleted", student.Id });
    }

    [HttpPost("student/restore/{id}")]
    public async Task<IActionResult> RestoreStudent(int id)
    {
        // Ignore query filter to find soft-deleted student
        var student = await _context.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student is null)
            return NotFound();

        student.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Student {student.Name} restored", student.Id });
    }

    [HttpGet("students/all")]
    public async Task<IActionResult> GetAllStudents()
    {
        // This includes soft-deleted students (admin view)
        var students = await _context.Students
            .IgnoreQueryFilters()
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.RegistrationNumber,
                s.IsActive,
                s.IsDeleted,
                LastUpdated = EF.Property<DateTime>(s, "LastUpdated")
            })
            .ToListAsync();

        return Ok(students);
    }

}