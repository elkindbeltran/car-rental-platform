namespace CarRental.SharedKernel.Application;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
