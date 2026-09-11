using CoffeeNChill.Functions.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CoffeeNChill.Functions.Functions;

/// <summary>
/// HTTP endpoints for staff document management (Blob Storage backed).
/// </summary>
/// <remarks>OWNER: C</remarks>
public class DocumentFunctions
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DocumentFunctions> _logger;

    public DocumentFunctions(IDocumentRepository documentRepository, ILogger<DocumentFunctions> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    // POST /api/documents/upload
    [Function("UploadStaffDocument")]
    public async Task<IActionResult> UploadStaffDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents/upload")] HttpRequest req,
        CancellationToken ct)
    {
        try
        {
            var file = req.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return new BadRequestObjectResult("No file uploaded.");

            if (string.IsNullOrWhiteSpace(file.ContentType))
                return new BadRequestObjectResult("File content type could not be determined.");

            using var stream = file.OpenReadStream();

            var info = await _documentRepository.UploadAsync(stream, file.FileName, file.ContentType, ct);

            return new OkObjectResult(new
            {
                message = $"File {info.FileName} uploaded successfully.",
                info.FileName,
                info.SizeBytes,
                info.LastModified,
                info.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return new StatusCodeResult(500);
        }
    }

    // GET /api/documents
    [Function("ListStaffDocuments")]
    public async Task<IActionResult> ListStaffDocuments(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req,
        CancellationToken ct)
    {
        try
        {
            var documents = await _documentRepository.ListAsync(ct);
            return new OkObjectResult(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing documents");
            return new StatusCodeResult(500);
        }
    }

    // GET /api/documents/download/{fileName}
    [Function("DownloadStaffDocument")]
    public async Task<IActionResult> DownloadStaffDocument(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents/download/{fileName}")] HttpRequest req,
        string fileName,
        CancellationToken ct)
    {
        try
        {
            var result = await _documentRepository.DownloadAsync(fileName, ct);
            if (result is null)
                return new NotFoundResult();

            return new FileStreamResult(result.Value.Content, result.Value.ContentType)
            {
                FileDownloadName = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {FileName}", fileName);
            return new StatusCodeResult(500);
        }
    }
}
