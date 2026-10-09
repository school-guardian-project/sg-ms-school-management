namespace ms_school_management.Api.School.Domain.Model;

public class SchoolCampus
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public Status Status { get; set; }
}
