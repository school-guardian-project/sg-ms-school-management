using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ms_school_management.Api.Infrastructure.DependencyInjection;
using ms_school_management.Api.Infrastructure.Grpc;
using ms_school_management.Api.Infrastructure.Auth;
using ms_school_management.Api.School.Application.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Kestrel endpoints are declared through configuration, not in code:
//   ASPNETCORE_Kestrel__Endpoints__Http__Url  -> 8080,  HTTP/1.1, public REST API
//   ASPNETCORE_Kestrel__Endpoints__Grpc__Url  -> 5001,  HTTP/2 (h2c), internal gRPC
// Both are injected from the deployment env file, exactly like every other .NET
// service in this project. Declaring them here as well duplicated each endpoint,
// so Kestrel aborted startup with "Address already in use" and the container
// crash-looped; Kong could not resolve its DNS and returned 503 for the whole
// /school-management route, breaking school/campus listings in the web app.

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Techo global (no depende de IP ni de usuario): acota el trabajo simultaneo
    // y la rafaga para que un flood desde una sola IP, o con IP falsificada via
    // cabeceras, nunca sature el backend. La concurrencia va primero para que un
    // rechazo de la ventana no consuma cupo ya adquirido.
    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
        PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetConcurrencyLimiter("global", _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 40, // ~ Max Pool Size (conexiones simultaneas a BD)
                QueueLimit = 0
            })),
        PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            })));

    // Escrituras (alta/edicion/borrado): 30/min por IP para frenar abuso.
    options.AddPolicy("writes", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 30,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<SchoolProfile>());
builder.Services.AddSchoolManagementServices(builder.Configuration);
builder.Services.AddGrpc();

// El servicio valida por su cuenta el access token RS256 de ms-iam (JWKS):
// la identidad sale del token, no de las cabeceras X-User-* del gateway.
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Todo endpoint REST exige un access token valido. El gRPC interno
// (SchoolManagementGrpcService, puerto 5001) no pasa por aqui.
app.MapControllers().RequireAuthorization();
app.MapGrpcService<SchoolManagementGrpcService>();

app.Run();
