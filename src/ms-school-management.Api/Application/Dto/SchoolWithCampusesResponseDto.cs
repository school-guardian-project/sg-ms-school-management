namespace ms_school_management.Api.School.Application.Dto;

public record SchoolWithCampusesResponseDto(
    Guid Id,
    string Name,
    IReadOnlyList<SchoolCampusResponseDto> Campuses);
