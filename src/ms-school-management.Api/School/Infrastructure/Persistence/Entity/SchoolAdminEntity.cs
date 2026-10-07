namespace ms_school_management.Api.School.Infrastructure.Persistence.Entity;

public class SchoolAdminEntity
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid ProfileId { get; set; }
    public string Status { get; set; }
}