

// var builder = WebApplication.CreateBuilder(args);

// // ============================================
// // SERVICES REGISTRATION
// // ============================================

// // Authentication & Authorization
// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", options => { });

// builder.Services.AddAuthorization();

// // Controllers
// builder.Services.AddControllers();

// // Exception handling with ProblemDetails
// builder.Services.AddProblemDetails();

// // OpenAPI for Scalar
// builder.Services.AddOpenApi();

// // ============================================
// // EXERCISE 2: DI Lifetimes (Captive Dependencies)
// // ============================================

// // Enable validation to catch captive dependencies
// builder.Host.UseDefaultServiceProvider(options =>
// {
//     options.ValidateScopes = true;
//     options.ValidateOnBuild = true;
// });

// // Register services
// builder.Services.AddSingleton<EnrollmentWorker>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// // ============================================
// // EXERCISE 3: Options Pattern with Validation
// // ============================================

// builder.Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();

// // ============================================
// // BUILD THE APPLICATION
// // ============================================

// var app = builder.Build();

// // ============================================
// // MIDDLEWARE PIPELINE
// // ============================================

// app.UseMiddleware<RequestLoggingMiddleware>();

// // Exception Handler - should be early
// app.UseExceptionHandler();

// // Status Code Pages - optional but recommended with ProblemDetails
// app.UseStatusCodePages();

// app.UseHttpsRedirection();
// app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();

// // ============================================
// // ENDPOINTS
// // ============================================

// // ============================================
// // EXERCISE 7: Environment Toggle (Dev vs Prod)
// // ============================================

// // Only expose OpenAPI and Scalar in Development
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
//     app.MapScalarApiReference();
// }

// // ============================================
// // EXERCISE 1: Protected Endpoint (from Session 1)
// // ============================================

// app.MapGet("/api/assessments/results", () =>
// {
//     return Results.Ok(new
//     {
//         courseCode = "CS-101",
//         studentId = "S-001",
//         letterGrade = "A"
//     });
// }).RequireAuthorization();

// // ============================================
// // EXERCISE 2: Worker Smoke Test
// // ============================================

// app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
// {
//     worker.ProcessBatch();
//     return Results.Ok("processed");
// });

// // ============================================
// // EXERCISE 3: Configuration Test
// // ============================================

// app.MapGet("/api/config/test", (IOptions<PaymentOptions> options) =>
// {
//     return Results.Ok(new
//     {
//         gatewayUrl = options.Value.GatewayUrl,
//         maxDepositBirr = options.Value.MaxDepositBirr
//     });
// });

// // ============================================
// // EXERCISE 6: Error Test Route
// // ============================================

// app.MapGet("/api/error", () =>
// {
//     throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
// });

// // ============================================
// // Map Controllers (MUST be after all endpoints)
// // ============================================

// app.MapControllers();

// app.Run();
/////////////////////////////////////
/// #########################################/////
//###### the above codes are for module 4 ####### //////
//###### the below codes are for module 5 #######//////
////////////////////////////////////////
/// 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICES REGISTRATION
// ============================================

// Authentication & Authorization
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", options => { });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Exception handling with ProblemDetails
builder.Services.AddProblemDetails();

// OpenAPI for Scalar
builder.Services.AddOpenApi();

// DI Lifetimes
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Options Pattern with Validation
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register DbContext
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

// ============================================
// BUILD THE APPLICATION
// ============================================

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ============================================
// SEED TEST DATA (This is where it goes!)
// ============================================

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    
    // Apply any pending migrations
    context.Database.Migrate();

    // Seed data if empty
    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m, IsActive = true }
        };
        context.Students.AddRange(students);
        context.SaveChanges();

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to Computer Science", Capacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", Capacity = 40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

// ============================================
// ENDPOINTS
// ============================================

// Environment Toggle
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Protected endpoint from Session 1
app.MapGet("/api/assessments/results", () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
}).RequireAuthorization();

// Worker smoke test
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

// Configuration test
app.MapGet("/api/config/test", (IOptions<PaymentOptions> options) =>
{
    return Results.Ok(new
    {
        gatewayUrl = options.Value.GatewayUrl,
        maxDepositBirr = options.Value.MaxDepositBirr
    });
});

// Error test route
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

// Map Controllers
app.MapControllers();

// ============================================
// RUN THE APPLICATION
// ============================================

app.Run();  // ← This is the last line