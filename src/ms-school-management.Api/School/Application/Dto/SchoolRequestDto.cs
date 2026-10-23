using System.ComponentModel.DataAnnotations;

namespace ms_school_management.Api.School.Application.Dto;

public record SchoolRequestDto(
    Guid CityId,
    byte[] Logo,
    string Name,
    [Required, StringLength(255, MinimumLength = 5)] string Address,
    [Range(typeof(decimal), "-90", "90")] decimal? Latitude,
    [Range(typeof(decimal), "-180", "180")] decimal? Longitude,
    long Phone,
    string Email,
    string? Website,
    string? Theme,
    string? Status = null);
