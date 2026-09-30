namespace ms_school_management.Api.School.Domain.Model;

public class Course
{
    public Guid Id { get; set; }
    public Guid CampusId { get; set; }
    public string Name { get; set; }
    public Status Status { get; set; }
}
