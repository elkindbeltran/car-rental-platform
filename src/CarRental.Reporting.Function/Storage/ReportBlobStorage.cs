using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace CarRental.Reporting.Worker.Storage;

public sealed class ReportBlobStorage(
    BlobServiceClient serviceClient,
    IOptions<BlobStorageOptions> options)
{
    public async Task<StoredReport> UploadAsync(
        Guid reportId,
        DateTimeOffset periodStartUtc,
        byte[] content,
        CancellationToken cancellationToken)
    {
        var container = serviceClient.GetBlobContainerClient(options.Value.ContainerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        await container.SetAccessPolicyAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        var blobName = $"{periodStartUtc:yyyy/MM}/{reportId:N}.pdf";
        var blob = container.GetBlobClient(blobName);
        using var stream = new MemoryStream(content, writable: false);
        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = "application/pdf" },
                Metadata = new Dictionary<string, string>
                {
                    ["reportId"] = reportId.ToString("N"),
                    ["periodStartUtc"] = periodStartUtc.ToString("O")
                }
            },
            cancellationToken);
        return new StoredReport(container.Name, blobName, content.LongLength);
    }

    public Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken) =>
        serviceClient.GetBlobContainerClient(options.Value.ContainerName)
            .DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken);
}
