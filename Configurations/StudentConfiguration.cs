using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.GPA)
            .HasPrecision(3, 2)
            .HasDefaultValue(0.0m);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.IsDeleted)
            .HasDefaultValue(false);

        // Shadow property for audit
        builder.Property<DateTime>("LastUpdated")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Configure xmin as concurrency token (PostgreSQL system column)
        builder.Property(s => s.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // Unique constraint
        builder.HasIndex(s => s.RegistrationNumber)
            .IsUnique();

        // Query filter for soft delete
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Relationships
        builder.HasMany(s => s.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Table name
        builder.ToTable("Students");
    }
}