namespace CarRental.SharedKernel.Results;

public class Result
{
    protected Result(bool isSuccess, ResultError error)
    {
        if (isSuccess && error != ResultError.None)
        {
            throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
        }

        if (!isSuccess && error == ResultError.None)
        {
            throw new ArgumentException("A failed result must contain an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ResultError Error { get; }

    public static Result Success() => new(true, ResultError.None);
    public static Result Failure(ResultError error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(ResultError error) => new(error);
}
