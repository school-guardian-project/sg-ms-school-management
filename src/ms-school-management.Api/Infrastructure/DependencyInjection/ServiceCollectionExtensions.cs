using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<ICreateSchoolUseCase, CreateSchoolService>();
        services.AddScoped<IGetSchoolUseCase, GetSchoolService>();
        services.AddScoped<IListSchoolsUseCase, ListSchoolsService>();
        services.AddScoped<IUpdateSchoolUseCase, UpdateSchoolService>();
        services.AddScoped<IDeleteSchoolUseCase, DeleteSchoolService>();

        return services;
    }
}
