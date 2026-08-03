namespace TmsApi.Entities;

public class Course
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Title { get; set; }
    public int MaxCapacity { get; set; }  // ← Renamed from Capacity
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}