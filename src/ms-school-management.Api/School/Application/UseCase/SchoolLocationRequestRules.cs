namespace ms_school_management.Api.School.Application.UseCase;

internal static class SchoolLocationRequestRules
{
    public static void Validate(decimal? latitude, decimal? longitude)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            throw new ArgumentException("School coordinates are out of range.");
        }

        if (latitude.HasValue != longitude.HasValue)
        {
            throw new ArgumentException("Both school coordinates must be supplied together.");
        }
    }
}
