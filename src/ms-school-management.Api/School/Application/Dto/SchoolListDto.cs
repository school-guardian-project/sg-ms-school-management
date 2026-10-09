namespace ms_school_management.Api.School.Application.Dto;

public record SchoolListDto(
    Guid Id,
    Guid CityId,
    string Name,
    string Address,
    string Status);
