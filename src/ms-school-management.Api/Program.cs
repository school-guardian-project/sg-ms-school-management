using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ms_school_management.Api.Infrastructure.DependencyInjection;
using ms_school_management.Api.Infrastructure.Grpc;
using ms_school_management.Api.Infrastructure.Tenancy;
using ms_school_management.Api.School.Infrastructure.Controller.Mapper;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, endpoint => endpoint.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(5001, endpoint => endpoint.Protocols = HttpProtocols.Http2);
});

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

// El gateway (Kong) valida el access token RS256 y reenvía la identidad en las
// cabeceras X-User-*. Aquí solo se confía en ellas para poblar HttpContext.User
// y filtrar por tenant; sin cabeceras el request sigue anónimo y compatible con
// los flujos internos (gRPC).
builder.Services.AddGatewayAuthentication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseRateLimiter();

app.MapControllers();
app.MapGrpcService<SchoolManagementGrpcService>();

app.Run();
