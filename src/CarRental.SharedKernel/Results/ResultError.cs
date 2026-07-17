namespace CarRental.SharedKernel.Results;

public sealed record ResultError
{
    private ResultError(string code, string description)
    {
        Code = code;
        Description = description;
    }

    public static ResultError None { get; } = new(string.Empty, string.Empty);

    public string Code { get; }
    public string Description { get; }

    public static ResultError Create(string code, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new ResultError(code, description);
    }
}
