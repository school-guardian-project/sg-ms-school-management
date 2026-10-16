using Microsoft.EntityFrameworkCore;
using ms_school_management.Api.School.Domain.Model;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Persistence.Context;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Repository;

public sealed class SchoolAdminRepositoryImpl : ISchoolAdminRepository
{
    private readonly SchoolManagementContext _context;

    public SchoolAdminRepositoryImpl(SchoolManagementContext context)
    {
        _context = context;
    }

    public async Task<SchoolAdmin?> FindByProfileIdAsync(Guid profileId, CancellationToken ct)
    {
        var entity = await _context.SchoolAdmins
            .AsNoTracking()
            .Where(sa => sa.ProfileId == profileId && sa.Status == "Active")
            .FirstOrDefaultAsync(ct);

        return entity is null ? null : ToModel(entity);
    }

    public async Task<SchoolAdmin> UpsertAsync(Guid profileId, Guid schoolId, CancellationToken ct)
    {
        var existing = await _context.SchoolAdmins
            .Where(sa => sa.ProfileId == profileId)
            .FirstOrDefaultAsync(ct);

        if (existing is null)
        {
            var created = new SchoolAdminEntity
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                ProfileId = profileId,
                Status = nameof(Status.Active)
            };

            _context.SchoolAdmins.Add(created);
            await _context.SaveChangesAsync(ct);

            return ToModel(created);
        }

        // Reintento del mismo evento o cambio de colegio: actualiza en vez de
        // insertar (UQ_SchoolAdmin_Profile lo prohibiria).
        existing.SchoolId = schoolId;
        existing.Status = nameof(Status.Active);
        await _context.SaveChangesAsync(ct);

        return ToModel(existing);
    }

    private static SchoolAdmin ToModel(SchoolAdminEntity e) => new()
    {
        Id = e.Id,
        SchoolId = e.SchoolId,
        ProfileId = e.ProfileId,
        Status = Enum.Parse<Status>(e.Status)
    };
}