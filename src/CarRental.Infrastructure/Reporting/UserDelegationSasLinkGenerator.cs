using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using CarRental.Application.Reporting.DownloadReport;
using CarRental.SharedKernel.Application;

namespace CarRental.Infrastructure.Reporting;

internal sealed class UserDelegationSasLinkGenerator(
    BlobServiceClient blobServiceClient,
    IDateTimeProvider dateTimeProvider) : IReportDownloadLinkGenerator, IDisposable
{
    private static readonly TimeSpan ClockSkewAllowance = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan DelegationKeyLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan DelegationKeyRefreshWindow = TimeSpan.FromMinutes(5);
    private readonly SemaphoreSlim _keyLock = new(1, 1);
    private UserDelegationKey? _delegationKey;
    private DateTimeOffset _delegationKeyExpiresAtUtc;

    public async Task<ReportDownloadLink> GenerateReadOnlyLinkAsync(
        ReportFileDescriptor report,
        TimeSpan lifetime,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(lifetime, TimeSpan.Zero);

        var now = dateTimeProvider.UtcNow;
        var expiresAtUtc = now.Add(lifetime);
        var delegationKey = await GetDelegationKeyAsync(now, cancellationToken);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = report.ContainerName,
            BlobName = report.BlobName,
            Resource = "b",
            Protocol = SasProtocol.Https,
            StartsOn = now.Subtract(ClockSkewAllowance),
            ExpiresOn = expiresAtUtc
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sas = sasBuilder.ToSasQueryParameters(
            delegationKey,
            blobServiceClient.AccountName);
        var blobUri = blobServiceClient
            .GetBlobContainerClient(report.ContainerName)
            .GetBlobClient(report.BlobName)
            .Uri;
        var uriBuilder = new UriBuilder(blobUri) { Query = sas.ToString() };

        return new ReportDownloadLink(uriBuilder.Uri, expiresAtUtc);
    }

    private async Task<UserDelegationKey> GetDelegationKeyAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (CanReuseDelegationKey(now))
        {
            return _delegationKey!;
        }

        await _keyLock.WaitAsync(cancellationToken);
        try
        {
            if (CanReuseDelegationKey(now))
            {
                return _delegationKey!;
            }

            var keyExpiresAtUtc = now.Add(DelegationKeyLifetime);
            var response = await blobServiceClient.GetUserDelegationKeyAsync(
                now.Subtract(ClockSkewAllowance),
                keyExpiresAtUtc,
                cancellationToken);

            _delegationKey = response.Value;
            _delegationKeyExpiresAtUtc = keyExpiresAtUtc;
            return _delegationKey;
        }
        finally
        {
            _keyLock.Release();
        }
    }

    private bool CanReuseDelegationKey(DateTimeOffset now) =>
        _delegationKey is not null &&
        now < _delegationKeyExpiresAtUtc.Subtract(DelegationKeyRefreshWindow);

    public void Dispose() => _keyLock.Dispose();
}
