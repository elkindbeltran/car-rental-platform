namespace CarRental.SharedKernel.Domain;

public interface IConcurrencyTracked
{
    byte[] RowVersion { get; set; }
}
