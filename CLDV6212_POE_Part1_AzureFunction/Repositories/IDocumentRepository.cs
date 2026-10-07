namespace CLDV6212_POE_Part1_AzureFunction.Repositories;

/// <summary>Metadata describing one blob in the staff-docs container.</summary>
public record StaffDocumentInfo(
    string FileName,
    long SizeBytes,
    DateTimeOffset? LastModified,
    string ContentType);

/// <summary>
/// Storage contract for staff operational documents.
///
/// Backed by Azure Blob Storage per the POE addendum (Sept 2026), which moved
/// document storage from Azure Files to Blob because Azurite does not emulate
/// the Files service. Blob IS emulated, so this runs entirely against the
/// local Azurite container.
/// </summary>
/// <remarks>CONTRACT — announce in the group chat before changing a signature.</remarks>
public interface IDocumentRepository
{
    Task<StaffDocumentInfo> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
    Task<IReadOnlyList<StaffDocumentInfo>> ListAsync(CancellationToken ct = default);
    Task<(Stream Content, string ContentType)?> DownloadAsync(string fileName, CancellationToken ct = default);
}
