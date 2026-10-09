using System.Text;
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

app.MapControllers();
app.MapGrpcService<SchoolManagementGrpcService>();

app.Run();
