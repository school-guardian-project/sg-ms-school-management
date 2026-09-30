namespace ms_school_management.Api.School.Infrastructure.Persistence.Entity;

public class CourseEntity
{
    public Guid Id { get; set; }
    public Guid CampusId { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
}
