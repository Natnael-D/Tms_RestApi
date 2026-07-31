using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using TmsApi.Configurations;  // Add this using

namespace TmsApi.Data;

public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);

        // If you need to add any additional global configurations, add them here
        // For example, set default schema or conventions

        base.OnModelCreating(modelBuilder);
    }
}