using System.ComponentModel.DataAnnotations;

namespace CarRental.Infrastructure.Reporting;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    [Required]
    [Url]
    public string ServiceUri { get; init; } = string.Empty;
}
