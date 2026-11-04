namespace ms_school_management.Api.School.Domain.Model;

public class School
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public byte[] Logo { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public long Phone { get; set; }
    public string Email { get; set; }
    public string? Website { get; set; }
    public string? Theme { get; set; }
    public Status Status { get; set; }
}
