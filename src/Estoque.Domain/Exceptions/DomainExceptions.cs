using Estoque.Domain.Common;

namespace Estoque.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(Error error, Exception? innerException = null) : base(error.Message, innerException)
    {
        Error = error;
    }

    public Error Error { get; }

    public string Code => Error.Code;
}

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string code, string message) : base(Error.Validation(code, message))
    {
    }
}

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(Error error) : base(error with { Type = ErrorType.NotFound })
    {
    }
}

public sealed class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(Error error) : base(error with { Type = ErrorType.Conflict })
    {
    }
}

public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string code, string message) : base(Error.Conflict(code, message))
    {
    }
}

public sealed class RepositoryException : DomainException
{
    public RepositoryException(string message, Exception innerException)
        : base(Error.Unexpected("Repositorio.FalhaPersistencia", message), innerException)
    {
    }
}
