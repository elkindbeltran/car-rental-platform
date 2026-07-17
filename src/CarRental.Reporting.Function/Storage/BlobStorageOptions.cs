using System.ComponentModel.DataAnnotations;

namespace CarRental.Reporting.Worker.Storage;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    [Required, Url]
    public string ServiceUri { get; init; } = string.Empty;

    [Required]
    public string ContainerName { get; init; } = "rental-reports";
}
