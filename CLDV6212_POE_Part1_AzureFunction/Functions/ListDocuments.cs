using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CoffeeNChill.Functions
{
    public class ListDocuments
    {

        /*Reference
        * Author:Microsoft Ignite
        * Title: BlobContainerClient.GetBlobsAsync(GetBlobsOptions, CancellationToken) Method
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient.getblobsasync?view=azure-dotnet
        */

        private readonly DocumentStorageService _documentStorageService;
        private readonly ILogger<ListDocuments> _logger;

        public ListDocuments(
            DocumentStorageService documentStorageService,
            ILogger<ListDocuments> logger)
        {
            _documentStorageService = documentStorageService;
            _logger = logger;
        }

        [Function("ListDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "documents")] HttpRequestData req)
        {
            try
            {
                //Get the Blob Storage container
                var containerClient =
                    _documentStorageService.GetContainerClient();

                var documents = new List<object>();

                //Get all blobs stored in the staff-docs container
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    documents.Add(new
                    {
                        fileName = blobItem.Name,
                        contentType = blobItem.Properties.ContentType,
                        size = blobItem.Properties.ContentLength,
                        lastModified = blobItem.Properties.LastModified
                    });
                }

                _logger.LogInformation(
                    "Successfully retrieved {DocumentCount} documents from staff-docs.",
                    documents.Count);

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(documents);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while listing documents from staff-docs.");

                var errorResponse = req.CreateResponse(
                    HttpStatusCode.InternalServerError);

                await errorResponse.WriteStringAsync(
                    "An error occurred while retrieving the documents.");

                return errorResponse;
            }
        }
    }
}