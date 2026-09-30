namespace ms_school_management.Api.School.Application.Dto;

public record SchoolRequestDto(
    Guid CityId,
    byte[] Logo,
    string Name,
    string Address,
    int Phone,
    string Email,
    string? Website,
    string? Theme);
