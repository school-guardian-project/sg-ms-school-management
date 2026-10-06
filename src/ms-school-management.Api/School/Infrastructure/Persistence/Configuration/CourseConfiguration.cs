using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Configuration;

public class CourseConfiguration : IEntityTypeConfiguration<CourseEntity>
{
    public void Configure(EntityTypeBuilder<CourseEntity> builder)
    {
        builder.ToTable("Course", "School");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
    }
}
