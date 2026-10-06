using ms_school_management.Api.Infrastructure.DependencyInjection;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<SchoolProfile>());
builder.Services.AddSchoolManagementServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();
