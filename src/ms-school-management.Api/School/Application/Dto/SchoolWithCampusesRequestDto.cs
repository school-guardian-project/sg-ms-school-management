using System.ComponentModel.DataAnnotations;

namespace ms_school_management.Api.School.Application.Dto;

public record SchoolWithCampusesRequestDto(
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
    [Required] IReadOnlyList<SchoolCampusRequestDto> Campuses,
    string? Status = null);

public record SchoolWithCampusesResponseDto(
    Guid Id,
    string Name,
    IReadOnlyList<SchoolCampusResponseDto> Campuses);

public record SchoolCampusResponseDto(
    Guid Id,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude);
