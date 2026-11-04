namespace ms_school_management.Api.School.Domain.Model;

/// <summary>
/// Vincula un perfil de administrador con el colegio que administra.
/// Un admin pertenece a exactamente un colegio: la unicidad de ProfileId la
/// impone la base de datos (<c>UQ_SchoolAdmin_Profile</c>).
/// </summary>
public class SchoolAdmin
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public Guid ProfileId { get; set; }
    public Status Status { get; set; }
}