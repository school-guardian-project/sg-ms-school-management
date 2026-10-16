using Microsoft.EntityFrameworkCore;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Persistence.Context;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Repository;

public sealed class CampusRepositoryImpl : ICampusRepository
{
    private readonly SchoolManagementContext _context;

    public CampusRepositoryImpl(SchoolManagementContext context)
    {
        _context = context;
    }

    public async Task<List<Domain.Model.SchoolCampus>> ListBySchoolIdAsync(Guid schoolId, CancellationToken ct)
    {
        var entities = await _context.Campuses
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.Status == "Active")
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return entities.Select(ToModel).ToList();
    }

    public async Task<Domain.Model.SchoolCampus?> FindByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Campuses
            .AsNoTracking()
            .Where(c => c.Id == id && c.Status == "Active")
            .FirstOrDefaultAsync(ct);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<Domain.Model.SchoolCampus?> FindBySchoolAndNameAsync(Guid schoolId, string name, CancellationToken ct)
    {
        var entity = await _context.Campuses
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId
                        && c.Name == name
                        && c.Status == "Active")
            .FirstOrDefaultAsync(ct);

        return entity is null ? null : ToModel(entity);
    }

    public async Task AddAsync(Domain.Model.SchoolCampus campus, CancellationToken ct)
    {
        _context.Campuses.Add(new SchoolCampusEntity
        {
            Id = campus.Id,
            SchoolId = campus.SchoolId,
            Name = campus.Name,
            Address = campus.Address,
            Status = campus.Status.ToString()
        });

        await _context.SaveChangesAsync(ct);
    }

    private static Domain.Model.SchoolCampus ToModel(SchoolCampusEntity e) => new()
    {
        Id = e.Id,
        SchoolId = e.SchoolId,
        Name = e.Name,
        Address = e.Address,
        Status = Enum.Parse<Domain.Model.Status>(e.Status)
    };
}