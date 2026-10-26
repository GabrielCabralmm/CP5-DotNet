namespace Estoque.Domain.Common;

public class OperationResult
{
    protected OperationResult(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Um resultado de sucesso não pode conter erro.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Um resultado de falha precisa conter um erro.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(Error error) => new(false, error);

    public static implicit operator OperationResult(Error error) => Failure(error);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(T? data, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        Data = data;
    }

    public T? Data { get; }

    public static OperationResult<T> Success(T data) => new(data, true, null);

    public static new OperationResult<T> Failure(Error error) => new(default, false, error);

    public OperationResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        IsSuccess ? OperationResult<TOut>.Success(map(Data!)) : OperationResult<TOut>.Failure(Error!);

    public static implicit operator OperationResult<T>(Error error) => Failure(error);
}
