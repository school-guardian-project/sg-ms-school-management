using Microsoft.AspNetCore.Authentication;

namespace ms_school_management.Api.Infrastructure.Tenancy;

/// <summary>
/// Opciones del esquema de autenticacion basado en las cabeceras del gateway.
/// </summary>
public sealed class GatewayAuthenticationOptions : AuthenticationSchemeOptions
{
}
