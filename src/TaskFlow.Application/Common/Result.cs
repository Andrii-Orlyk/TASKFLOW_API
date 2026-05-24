namespace TaskFlow.Application.Common;

public class Result
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; protected init; }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(Error error) => new() { IsSuccess = false, Error = error };
}

public sealed class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public new static Result<T> Failure(Error error) => new() { IsSuccess = false, Error = error };
}
