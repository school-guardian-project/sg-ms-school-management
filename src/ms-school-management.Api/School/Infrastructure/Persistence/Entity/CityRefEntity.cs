namespace ms_school_management.Api.School.Infrastructure.Persistence.Entity;

/// <summary>
/// Solo lectura de Geographic.City (mismo SQL Server): School solo guarda CityId.
/// </summary>
public class CityRefEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
