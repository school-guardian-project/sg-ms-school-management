using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using ms_school_management.Api.Infrastructure.DependencyInjection;
using ms_school_management.Api.Infrastructure.Grpc;
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

// JWT HS256 emitido por ms-iam (sin issuer/audience). Solo se valida la
// firma y la expiracion para poblar HttpContext.User y filtrar por tenant;
// no hay endpoints [Authorize] ni UseAuthorization: sin token el request
// sigue siendo anonimo y compatible con los flujos internos (gRPC).
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]
    ?? "your-256-bit-base64-encoded-secret-min-32-chars";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

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
