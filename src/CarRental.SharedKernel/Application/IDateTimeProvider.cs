namespace CarRental.SharedKernel.Application;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
