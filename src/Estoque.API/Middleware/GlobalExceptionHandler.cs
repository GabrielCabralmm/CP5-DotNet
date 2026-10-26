using Estoque.API.Extensions;
using Estoque.Domain.Common;
using Estoque.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Estoque.API.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        Error error;

        if (exception is DomainException domainException)
        {
            logger.LogWarning(exception, "Erro de domínio não tratado: {Code}", domainException.Code);
            error = domainException.Error;
        }
        else
        {
            logger.LogError(exception, "Erro inesperado ao processar {Path}", httpContext.Request.Path);
            error = Error.Unexpected("Erro.Inesperado", "Ocorreu um erro inesperado ao processar a requisição.");
        }

        await httpContext.WriteProblemAsync(error, cancellationToken);
        return true;
    }
}
