using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ms_school_management.Api.Infrastructure.Tenancy;
using ms_school_management.Api.School.Application.Search;
using ms_school_management.Api.School.Application.Search.Strategy;
using ms_school_management.Api.School.Application.UseCase;
using ms_school_management.Api.School.Domain.Ports.In;
using ms_school_management.Api.School.Domain.Ports.Out;
using ms_school_management.Api.School.Infrastructure.Persistence.Context;
using ms_school_management.Api.School.Infrastructure.Persistence.Repository;

namespace ms_school_management.Api.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchoolManagementServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SchoolManagementContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISchoolRepository, SchoolRepositoryImpl>();
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<ITenantSchoolResolver, TenantSchoolResolver>();
        services.AddScoped<ICreateSchoolUseCase, CreateSchoolService>();
        services.AddScoped<ICreateSchoolWithCampusesUseCase, CreateSchoolWithCampusesService>();
        services.AddScoped<IGetSchoolUseCase, GetSchoolService>();
        services.AddScoped<IListSchoolsUseCase, ListSchoolsService>();
        services.AddScoped<IUpdateSchoolUseCase, UpdateSchoolService>();
        services.AddScoped<IUpdateSchoolWithCampusesUseCase, UpdateSchoolWithCampusesService>();
        services.AddScoped<IListSchoolCampusesUseCase, ListSchoolCampusesService>();
        services.AddScoped<IDeleteSchoolUseCase, DeleteSchoolService>();

        services.AddScoped<ISchoolSearchStrategy, NameSearchStrategy>();
        services.AddScoped<ISearchSchoolsUseCase, SearchSchoolsService>();

        services.AddScoped<ICampusRepository, CampusRepositoryImpl>();
        services.AddScoped<IListCampusesBySchoolUseCase, ListCampusesBySchoolService>();

        services.AddScoped<ISchoolAdminRepository, SchoolAdminRepositoryImpl>();
        services.AddScoped<ILinkAdminSchoolUseCase, LinkAdminSchoolService>();
        services.AddScoped<IGetAdminSchoolUseCase, GetAdminSchoolService>();
        services.AddScoped<IGetCampusUseCase, GetCampusService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
