using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ms_school_management.Api.School.Domain.Exceptions;

namespace ms_school_management.Api.Infrastructure.Auth;

/// <summary>
/// Traduce <see cref="SchoolAccessDeniedException"/> (regla de pertenencia de la
/// capa de aplicacion) a 403 con ProblemDetails.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SchoolAccessDeniedFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not SchoolAccessDeniedException) return;

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "No tienes acceso a este colegio.",
        })
        {
            StatusCode = StatusCodes.Status403Forbidden,
        };
        context.ExceptionHandled = true;
    }
}
