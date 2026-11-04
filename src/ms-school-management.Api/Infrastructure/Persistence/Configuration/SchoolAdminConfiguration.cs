using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Configuration;

public class SchoolAdminConfiguration : IEntityTypeConfiguration<SchoolAdminEntity>
{
    public void Configure(EntityTypeBuilder<SchoolAdminEntity> builder)
    {
        builder.ToTable("SchoolAdmin", "School");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();

        // La FK a Iam.Profile no se mapea con navegación: el perfil es de otro
        // microservicio y EF no debe Inferir una relacion entre esquemas que no
        // administra este. La base de datos ya la declara (FK_SchoolAdmin_Profile).
        builder.HasIndex(x => x.ProfileId).IsUnique();
    }
}