namespace ms_school_management.Api.School.Application.Dto;

public record SchoolCampusResponseDto(
    Guid Id,
    string Name,
    string Address,
    decimal? Latitude,
    decimal? Longitude);
