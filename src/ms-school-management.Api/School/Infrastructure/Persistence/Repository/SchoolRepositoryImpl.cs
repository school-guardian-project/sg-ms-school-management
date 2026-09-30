using Microsoft.EntityFrameworkCore;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Persistence.Context;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Repository;

public sealed class SchoolRepositoryImpl : ISchoolRepository
{
    private readonly SchoolManagementContext _context;

    public SchoolRepositoryImpl(SchoolManagementContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Domain.Model.School school)
    {
        var entity = new SchoolEntity
        {
            Id = school.Id,
            CityId = school.CityId,
            Logo = school.Logo,
            Name = school.Name,
            Address = school.Address,
            Phone = school.Phone,
            Email = school.Email,
            Website = school.Website,
            Theme = school.Theme,
            Status = school.Status.ToString()
        };
        await _context.Schools.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Domain.Model.School?> FindByIdAsync(Guid id)
    {
        var e = await _context.Schools.FindAsync(id);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<IReadOnlyList<Domain.Model.School>> FindAllAsync()
    {
        var entities = await _context.Schools.ToListAsync();
        return entities.Select(MapToDomain).ToList();
    }

    public async Task UpdateAsync(Domain.Model.School school)
    {
        var e = await _context.Schools.FindAsync(school.Id);
        if (e is null) throw new KeyNotFoundException($"School {school.Id} not found");

        e.Name = school.Name;
        e.Address = school.Address;
        e.Phone = school.Phone;
        e.Email = school.Email;
        e.Website = school.Website;
        e.Theme = school.Theme;
        e.Status = school.Status.ToString();

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _context.Schools.FindAsync(id);
        if (e is null) throw new KeyNotFoundException($"School {id} not found");

        _context.Schools.Remove(e);
        await _context.SaveChangesAsync();
    }

    private static Domain.Model.School MapToDomain(SchoolEntity e) => new()
    {
        Id = e.Id,
        CityId = e.CityId,
        Logo = e.Logo,
        Name = e.Name,
        Address = e.Address,
        Phone = e.Phone,
        Email = e.Email,
        Website = e.Website,
        Theme = e.Theme,
        Status = Enum.Parse<Status>(e.Status)
    };
}
