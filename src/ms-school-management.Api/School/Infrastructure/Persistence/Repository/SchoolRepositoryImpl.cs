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
        await _context.Schools.AddAsync(ToEntity(school));
        await _context.SaveChangesAsync();
    }

    public async Task SaveWithCampusesAsync(Domain.Model.School school, IReadOnlyList<SchoolCampus> campuses)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.Schools.AddAsync(ToEntity(school));
        await _context.Campuses.AddRangeAsync(campuses.Select(ToEntity));
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
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

        e.CityId = school.CityId;
        e.Logo = school.Logo;
        e.Name = school.Name;
        e.Address = school.Address;
        e.Latitude = school.Latitude;
        e.Longitude = school.Longitude;
        e.Phone = school.Phone;
        e.Email = school.Email;
        e.Website = school.Website;
        e.Theme = school.Theme;
        e.Status = school.Status.ToString();

        await _context.SaveChangesAsync();
    }

    public async Task UpdateWithCampusesAsync(Domain.Model.School school, IReadOnlyList<SchoolCampus> campuses)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var entity = await _context.Schools.FindAsync(school.Id)
            ?? throw new KeyNotFoundException($"School {school.Id} not found");

        entity.CityId = school.CityId;
        entity.Logo = school.Logo;
        entity.Name = school.Name;
        entity.Address = school.Address;
        entity.Latitude = school.Latitude;
        entity.Longitude = school.Longitude;
        entity.Phone = school.Phone;
        entity.Email = school.Email;
        entity.Website = school.Website;
        entity.Theme = school.Theme;
        entity.Status = school.Status.ToString();

        foreach (var campus in campuses)
        {
            SchoolCampusEntity campusEntity;
            if (campus.Id == Guid.Empty)
            {
                campusEntity = new SchoolCampusEntity
                {
                    Id = Guid.NewGuid(),
                    SchoolId = school.Id,
                    Status = campus.Status.ToString(),
                };
                await _context.Campuses.AddAsync(campusEntity);
            }
            else
            {
                campusEntity = await _context.Campuses
                    .SingleOrDefaultAsync(x => x.Id == campus.Id && x.SchoolId == school.Id)
                    ?? throw new KeyNotFoundException($"Campus {campus.Id} does not belong to school {school.Id}");
            }

            campusEntity.Name = campus.Name;
            campusEntity.Address = campus.Address;
            campusEntity.Latitude = campus.Latitude;
            campusEntity.Longitude = campus.Longitude;
            campusEntity.Status = campus.Status.ToString();
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<SchoolCampus>> FindCampusesBySchoolAsync(Guid schoolId)
    {
        var campuses = await _context.Campuses
            .Where(x => x.SchoolId == schoolId && x.Status == Status.Active.ToString())
            .OrderBy(x => x.Name)
            .ToListAsync();
        return campuses.Select(MapToDomain).ToList();
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
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        Phone = e.Phone,
        Email = e.Email,
        Website = e.Website,
        Theme = e.Theme,
        Status = Enum.Parse<Status>(e.Status)
    };

    private static SchoolEntity ToEntity(Domain.Model.School school) => new()
    {
        Id = school.Id,
        CityId = school.CityId,
        Logo = school.Logo,
        Name = school.Name,
        Address = school.Address,
        Latitude = school.Latitude,
        Longitude = school.Longitude,
        Phone = school.Phone,
        Email = school.Email,
        Website = school.Website,
        Theme = school.Theme,
        Status = school.Status.ToString(),
    };

    private static SchoolCampusEntity ToEntity(SchoolCampus campus) => new()
    {
        Id = campus.Id,
        SchoolId = campus.SchoolId,
        Name = campus.Name,
        Address = campus.Address,
        Latitude = campus.Latitude,
        Longitude = campus.Longitude,
        Status = campus.Status.ToString(),
    };

    private static SchoolCampus MapToDomain(SchoolCampusEntity campus) => new()
    {
        Id = campus.Id,
        SchoolId = campus.SchoolId,
        Name = campus.Name,
        Address = campus.Address,
        Latitude = campus.Latitude,
        Longitude = campus.Longitude,
        Status = Enum.Parse<Status>(campus.Status),
    };
}
