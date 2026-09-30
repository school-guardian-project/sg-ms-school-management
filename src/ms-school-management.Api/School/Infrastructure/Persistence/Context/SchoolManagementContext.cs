using Microsoft.EntityFrameworkCore;
using ms_school_management.Api.School.Infrastructure.Persistence.Configuration;
using ms_school_management.Api.School.Infrastructure.Persistence.Entity;

namespace ms_school_management.Api.School.Infrastructure.Persistence.Context;

public class SchoolManagementContext : DbContext
{
    public SchoolManagementContext(DbContextOptions<SchoolManagementContext> options) : base(options) { }

    public DbSet<SchoolEntity> Schools => Set<SchoolEntity>();
    public DbSet<SchoolCampusEntity> Campuses => Set<SchoolCampusEntity>();
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new SchoolConfiguration());
        modelBuilder.ApplyConfiguration(new SchoolCampusConfiguration());
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
    }
}
