namespace CarRental.Reporting.Worker.Storage;

public sealed record StoredReport(string ContainerName, string BlobName, long ContentLength);
