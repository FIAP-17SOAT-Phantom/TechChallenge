namespace OficinaMecanica.Domain.Common;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    BusinessRule = 4,
    Unauthorized = 5,
    Forbidden = 6
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, string error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, string.Empty, ErrorType.None);
    public static Result Failure(string error, ErrorType errorType = ErrorType.BusinessRule) => new(false, error, errorType);
    public static Result NotFound(string error) => Failure(error, ErrorType.NotFound);
    public static Result Conflict(string error) => Failure(error, ErrorType.Conflict);
    public static Result Unauthorized(string error) => Failure(error, ErrorType.Unauthorized);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty, ErrorType.None);
    public static Result<T> Failure<T>(string error, ErrorType errorType = ErrorType.BusinessRule) => new(default!, false, error, errorType);
    public static Result<T> NotFound<T>(string error) => Failure<T>(error, ErrorType.NotFound);
    public static Result<T> Conflict<T>(string error) => Failure<T>(error, ErrorType.Conflict);
    public static Result<T> Unauthorized<T>(string error) => Failure<T>(error, ErrorType.Unauthorized);
}

public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error, ErrorType errorType)
    : base(isSuccess, error, errorType)
    {
        Value = value;
    }
}
