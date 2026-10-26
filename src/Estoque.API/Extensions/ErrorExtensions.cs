using Estoque.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Extensions;

public static class ErrorExtensions
{
    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.ExternalService => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError
    };

    public static ProblemDetails ToProblemDetails(this Error error, HttpContext httpContext)
    {
        var status = error.Type.ToStatusCode();
        var problem = new ProblemDetails
        {
            Status = status,
            Title = Titulo(error.Type),
            Detail = error.Message,
            Type = $"https://httpstatuses.io/{status}",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["success"] = false;
        problem.Extensions["code"] = error.Code;
        problem.Extensions["errorType"] = error.Type.ToString();
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }

    public static Task WriteProblemAsync(this HttpContext httpContext, Error error, CancellationToken ct = default)
    {
        var problem = error.ToProblemDetails(httpContext);
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken: ct);
    }

    private static string Titulo(ErrorType type) => type switch
    {
        ErrorType.Validation => "Requisição inválida",
        ErrorType.NotFound => "Recurso não encontrado",
        ErrorType.Conflict => "Conflito de regra de negócio",
        ErrorType.Unauthorized => "Não autenticado",
        ErrorType.Forbidden => "Acesso negado",
        ErrorType.ExternalService => "Falha em serviço externo",
        _ => "Erro inesperado"
    };
}
