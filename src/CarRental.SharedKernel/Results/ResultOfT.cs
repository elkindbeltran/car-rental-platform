namespace CarRental.SharedKernel.Results;

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T value) : base(true, ResultError.None)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    internal Result(ResultError error) : base(false, error) { }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ResultError, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}
