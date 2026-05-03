namespace HospitalApp.Core.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public string? Error { get; private init; }
    public string? Warning { get; private init; }
    public int StatusCode { get; private init; }

    private Result(bool isSuccess, T? data, string? error, int statusCode, string? warning = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
        Warning = warning;
    }

    public static Result<T> Success(T data, int statusCode = 200) =>
        new(true, data, null, statusCode);

    public static Result<T> Created(T data, string? warning = null) =>
        new(true, data, null, 201, warning);

    public static Result<T> Failure(string error, int statusCode = 400) =>
        new(false, default, error, statusCode);

    public static Result<T> NotFound(string error = "Resource not found.") =>
        new(false, default, error, 404);

    public static Result<T> Unauthorized(string error = "Unauthorized.") =>
        new(false, default, error, 401);

    public static Result<T> Forbidden(string error = "Forbidden.") =>
        new(false, default, error, 403);
}

public class Result
{
    public bool IsSuccess { get; private init; }
    public string? Error { get; private init; }
    public int StatusCode { get; private init; }

    private Result(bool isSuccess, string? error, int statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success(int statusCode = 200) => new(true, null, statusCode);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    public static Result NotFound(string error = "Resource not found.") => new(false, error, 404);
}
