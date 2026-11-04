namespace ms_school_management.Api.School.Application.Dto;

public record SchoolResponseDto(
    Guid Id,
    Guid CityId,
    byte[] Logo,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude,
    long Phone,
    string Email,
    string? Website,
    string? Theme,
    string Status,
    string CityName = "");
