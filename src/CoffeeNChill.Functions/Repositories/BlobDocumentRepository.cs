namespace CoffeeNChill.Functions.Repositories;

/// <summary>
/// Azure Blob Storage implementation of <see cref="IDocumentRepository"/>,
/// backed by the "staff-docs" container.
/// </summary>
/// <remarks>
/// OWNER: C — STUB. See issue [C] "implement BlobDocumentRepository".
///
/// Per the POE addendum, documents live in Blob Storage rather than an Azure
/// File Share. Blob IS emulated by Azurite, so this connects to the same local
/// emulator as the MenuItems table — no live Azure account required.
///
/// Implementation notes:
///  - BlobContainerClient from BlobStorageConnection, CreateIfNotExists() in ctor
///  - Upload: blob.UploadAsync(stream, overwrite) with BlobHttpHeaders.ContentType
///    (no manual chunking needed — the SDK blocks large uploads automatically)
///  - List:   container.GetBlobsAsync() returns Name, ContentLength, LastModified
///            and ContentType in BlobItem.Properties, so no extra call per blob
///  - Download: blob.DownloadStreamingAsync() returns the network stream directly
/// </remarks>
public class BlobDocumentRepository : IDocumentRepository
{
    // TODO(C): inject IConfiguration + ILogger, build a BlobContainerClient
    //          from BlobStorageConnection + StaffDocsContainerName,
    //          call CreateIfNotExists() here.

    public Task<StaffDocumentInfo> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
        => throw new NotImplementedException("BlobDocumentRepository.UploadAsync — see issue [C]");

    public Task<IReadOnlyList<StaffDocumentInfo>> ListAsync(CancellationToken ct = default)
        => throw new NotImplementedException("BlobDocumentRepository.ListAsync — see issue [C]");

    public Task<(Stream Content, string ContentType)?> DownloadAsync(string fileName, CancellationToken ct = default)
        => throw new NotImplementedException("BlobDocumentRepository.DownloadAsync — see issue [C]");
}
