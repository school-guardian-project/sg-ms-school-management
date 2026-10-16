namespace ms_school_management.Api.School.Infrastructure.Persistence.Entity;

public class SchoolCampusEntity
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Status { get; set; }
}
