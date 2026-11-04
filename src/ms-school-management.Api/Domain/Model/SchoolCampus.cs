namespace ms_school_management.Api.School.Domain.Model;

public class SchoolCampus
{
    public const string CentralName = "Sede central";
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public Status Status { get; set; }

    public static SchoolCampus Central(School school) => new()
    {
        Id = Guid.NewGuid(),
        SchoolId = school.Id,
        Name = CentralName,
        Address = school.Address,
        Latitude = school.Latitude,
        Longitude = school.Longitude,
        Status = Status.Active
    };
}
