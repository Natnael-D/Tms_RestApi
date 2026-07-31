using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Capacity)
            .IsRequired()
            .HasDefaultValue(30);

        // Unique constraint on Code (natural key)
        builder.HasIndex(c => c.Code)
            .IsUnique();

        // Relationships
        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);  // Don't delete course if it has enrollments

        // Table name
        builder.ToTable("Courses");
    }
}