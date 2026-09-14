using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class DownloadDocument
    {

        /*Reference
        * Author:Microsoft Ignite
        * Title: BlobBaseClient.ExistsAsync(CancellationToken) Method
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.specialized.blobbaseclient.existsasync?view=azure-dotnet
        */

        private readonly DocumentStorageService _documentStorageService;
        private readonly ILogger<DownloadDocument> _logger;

        public DownloadDocument(
            DocumentStorageService documentStorageService,
            ILogger<DownloadDocument> logger)
        {
            _documentStorageService = documentStorageService;
            _logger = logger;
        }

        [Function("DownloadDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "documents/download/{fileName}")] HttpRequestData req,
            string fileName)
        {
            try
            {
                //Get the Blob Storage container
                var containerClient =
                    _documentStorageService.GetContainerClient();

                //Get the requested blob
                var blobClient = containerClient.GetBlobClient(fileName);

                //Check if the file exists
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning(
                        "Document {FileName} was not found.",
                        fileName);

                    var notFound = req.CreateResponse(HttpStatusCode.NotFound);

                    await notFound.WriteStringAsync(
                        "Document not found.");

                    return notFound;
                }

                //Download the blob
                var download = await blobClient.DownloadAsync();

                var response = req.CreateResponse(HttpStatusCode.OK);

                //Set the file name in the response
                response.Headers.Add(
                    "Content-Disposition",
                    $"attachment; filename=\"{fileName}\"");

                //Set the content type if one is available
                if (!string.IsNullOrEmpty(download.Value.Details.ContentType))
                {
                    response.Headers.Add(
                        "Content-Type",
                        download.Value.Details.ContentType);
                }

                //Copy the blob contents into the response
                await download.Value.Content.CopyToAsync(response.Body);

                _logger.LogInformation(
                    "Document {FileName} downloaded successfully.",
                    fileName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while downloading document {FileName}.",
                    fileName);

                var errorResponse = req.CreateResponse(
                    HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while downloading the document.");

                return errorResponse;
            }
        }
    }
}