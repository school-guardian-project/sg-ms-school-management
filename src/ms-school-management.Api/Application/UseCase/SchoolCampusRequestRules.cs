using ms_school_management.Api.School.Application.Dto;

namespace ms_school_management.Api.School.Application.UseCase;

internal static class SchoolCampusRequestRules
{
    public static void Validate(IReadOnlyList<SchoolCampusRequestDto> campuses)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ids = new HashSet<Guid>();
        foreach (var campus in campuses)
        {
            if (campus.Status is not ("Active" or "Inactive")
                || (campus.Status == "Inactive" && (!campus.Id.HasValue || campus.Id == Guid.Empty)))
            {
                throw new ArgumentException("Campus status must be Active or Inactive; inactive campuses require an existing id.", nameof(campuses));
            }
            if (string.IsNullOrWhiteSpace(campus.Name) || campus.Name.Length > 30)
            {
                throw new ArgumentException("Campus names must contain 1 to 30 characters.", nameof(campuses));
            }

            if (string.IsNullOrWhiteSpace(campus.Address) || campus.Address.Length < 5 || campus.Address.Length > 255)
            {
                throw new ArgumentException("Campus addresses must contain 5 to 255 characters.", nameof(campuses));
            }

            if (campus.Status == "Active" && !names.Add(campus.Name.Trim()))
            {
                throw new ArgumentException($"Campus name '{campus.Name}' is duplicated.", nameof(campuses));
            }

            if (campus.Id is Guid id && !ids.Add(id))
            {
                throw new ArgumentException("A campus was submitted more than once.", nameof(campuses));
            }

            if (campus.Latitude is < -90 or > 90 || campus.Longitude is < -180 or > 180)
            {
                throw new ArgumentException("Campus coordinates are out of range.", nameof(campuses));
            }

            if (campus.Latitude.HasValue != campus.Longitude.HasValue)
            {
                throw new ArgumentException("Both campus coordinates must be supplied together.", nameof(campuses));
            }
        }
    }
}
