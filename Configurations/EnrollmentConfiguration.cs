using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.StudentId)
            .IsRequired();

        builder.Property(e => e.CourseId)
            .IsRequired();

        builder.Property(e => e.Grade)
            .HasPrecision(3, 2)
            .IsRequired(false);

        builder.Property(e => e.EnrolledAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // NEW: IsArchived property
        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);

        // Unique constraint
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique()
            .HasDatabaseName("IX_Enrollments_Student_Course");

        // Relationships
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Table name
        builder.ToTable("Enrollments");
    }
}