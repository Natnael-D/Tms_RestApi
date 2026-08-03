using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Filters;        // ← NEW
using TmsApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICES REGISTRATION
// ============================================

// Authentication & Authorization
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", options => { });

builder.Services.AddAuthorization();

// Controllers with Audit Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();  // ← NEW: Global audit filter
});

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

// Register Services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

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
// SEED DATA (ONLY IN DEVELOPMENT)
// ============================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
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

app.Run();