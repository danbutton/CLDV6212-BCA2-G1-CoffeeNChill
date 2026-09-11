using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Repositories;

/// <summary>
/// Azure Blob Storage implementation of <see cref="IDocumentRepository"/>,
/// backed by the "staff-docs" container.
/// </summary>
/// <remarks>
/// OWNER: C
///
/// Per the POE addendum, documents live in Blob Storage rather than an Azure
/// File Share. Blob IS emulated by Azurite, so this connects to the same local
/// emulator as the MenuItems table — no live Azure account required.
/// </remarks>
public class BlobDocumentRepository : IDocumentRepository
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobDocumentRepository> _logger;

    public BlobDocumentRepository(IConfiguration configuration, ILogger<BlobDocumentRepository> logger)
    {
        _logger = logger;

        string connectionString = configuration["BlobStorageConnection"]
                                  ?? configuration["AzureWebJobsStorage"]
                                  ?? "UseDevelopmentStorage=true";

        string containerName = configuration["StaffDocsContainerName"] ?? "staff-docs";

        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<StaffDocumentInfo> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            var headers = new BlobHttpHeaders { ContentType = contentType };

            await blobClient.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = headers
            }, ct);

            var props = await blobClient.GetPropertiesAsync(cancellationToken: ct);

            return new StaffDocumentInfo(
                fileName,
                props.Value.ContentLength,
                props.Value.LastModified,
                props.Value.ContentType ?? contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload blob {FileName}", fileName);
            throw;
        }
    }

    public async Task<IReadOnlyList<StaffDocumentInfo>> ListAsync(CancellationToken ct = default)
    {
        try
        {
            var results = new List<StaffDocumentInfo>();

            await foreach (BlobItem blob in _containerClient.GetBlobsAsync(cancellationToken: ct))
            {
                results.Add(new StaffDocumentInfo(
                    blob.Name,
                    blob.Properties.ContentLength ?? 0,
                    blob.Properties.LastModified,
                    blob.Properties.ContentType ?? "application/octet-stream"));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list blobs in {Container}", _containerClient.Name);
            throw;
        }
    }

    public async Task<(Stream Content, string ContentType)?> DownloadAsync(
        string fileName,
        CancellationToken ct = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(fileName);

            if (!await blobClient.ExistsAsync(ct))
            {
                _logger.LogWarning("Blob {FileName} not found", fileName);
                return null;
            }

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);

            string contentType = response.Value.Details.ContentType ?? "application/octet-stream";

            return (response.Value.Content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob {FileName}", fileName);
            throw;
        }
    }
}