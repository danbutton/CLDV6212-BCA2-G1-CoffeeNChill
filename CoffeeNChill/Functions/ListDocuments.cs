using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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

        public ListDocuments(DocumentStorageService documentStorageService)
        {
            _documentStorageService = documentStorageService;
        }

        [Function("ListDocuments")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "documents")] HttpRequestData req)
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

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(documents);

            return response;
        }
    }
}
