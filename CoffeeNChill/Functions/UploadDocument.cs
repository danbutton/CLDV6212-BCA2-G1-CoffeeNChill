using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions
{
    public class UploadDocument
    {

        /*Reference
        * Author:Microsoft Ignite
        * Title: BlobClient.UploadAsync Method
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.uploadasync?view=azure-dotnet
        */

        /*Reference
        * Author: Microsoft
        * Title: LoggerExtensions Class
        * Link: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions?view=net-8.0
        */

        private readonly DocumentStorageService _documentStorageService;
        private readonly ILogger<UploadDocument> _logger;

        public UploadDocument(
            DocumentStorageService documentStorageService,
            ILogger<UploadDocument> logger)
        {
            _documentStorageService = documentStorageService;
            _logger = logger;
        }

        [Function("UploadDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "documents/upload")] HttpRequestData req)
        {
            try
            {
                //Get Blob Storage container
                var containerClient =
                    _documentStorageService.GetContainerClient();

                //This endpoint expects a file in the request body
                if (!req.Headers.TryGetValues("x-file-name", out var fileNameValues))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync(
                        "Please provide the file name using the x-file-name header.");

                    _logger.LogWarning(
                        "Document upload rejected because no file name was provided.");

                    return badRequest;
                }

                string fileName = fileNameValues.First();

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("File name cannot be empty.");

                    _logger.LogWarning(
                        "Document upload rejected because the file name was empty.");

                    return badRequest;
                }

                /*Reference
                 * Author: Microsoft
                 * Title: BlobHttpHeaders Class
                 * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.models.blobhttpheaders?view=azure-dotnet
                 */

                //Determine the MIME type from the request header or file extension
                string contentType = string.Empty;

                if (req.Headers.TryGetValues("Content-Type", out var contentTypeValues))
                {
                    contentType = contentTypeValues.First()
                        .Split(';')[0]
                        .Trim()
                        .ToLowerInvariant();
                }

                //Allow common staff document file types
                var allowedContentTypes = new[]
                {
                    "text/plain",
                    "application/pdf",
                    "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };

                //If the request does not provide a useful content type,
                //determine it from the file extension
                if (string.IsNullOrWhiteSpace(contentType) ||
                    contentType == "application/octet-stream" ||
                    contentType == "application/x-www-form-urlencoded")
                {
                    string extension =
                        Path.GetExtension(fileName).ToLowerInvariant();

                    contentType = extension switch
                    {
                        ".txt" => "text/plain",
                        ".pdf" => "application/pdf",
                        ".doc" => "application/msword",
                        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        _ => "application/octet-stream"
                    };
                }

                //Validate the MIME type
                if (!allowedContentTypes.Contains(contentType))
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);

                    await badRequest.WriteStringAsync(
                        "Unsupported file type. Allowed file types are TXT, PDF, DOC and DOCX.");

                    _logger.LogWarning(
                        "Document upload rejected because MIME type {ContentType} is not allowed.",
                        contentType);

                    return badRequest;
                }

                //Upload the request body directly to Blob Storage
                var blobClient = containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(
                    req.Body,
                    overwrite: true);

                //Store the MIME type with the blob
                await blobClient.SetHttpHeadersAsync(
                    new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = contentType
                    });

                _logger.LogInformation(
                    "Document {FileName} uploaded successfully with MIME type {ContentType}.",
                    fileName,
                    contentType);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Document uploaded successfully.",
                    fileName = fileName,
                    contentType = contentType
                });

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                ex,
                "An error occurred while uploading a document.");

                var errorResponse = req.CreateResponse(
                    HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while uploading the document.");

                return errorResponse;
            }
        }
    }
}
