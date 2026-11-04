using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Configuration;

public class SchoolCampusConfiguration : IEntityTypeConfiguration<SchoolCampusEntity>
{
    public void Configure(EntityTypeBuilder<SchoolCampusEntity> builder)
    {
        builder.ToTable("SchoolCampus", "School");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Latitude).HasPrecision(10, 7);
        builder.Property(x => x.Longitude).HasPrecision(10, 7);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
    }
}
