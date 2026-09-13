using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using CoffeeNChill.Services;

namespace CoffeeNChill.Functions
{
    public class UploadDocument
    {
        private readonly DocumentStorageService _documentStorageService;

        public UploadDocument(DocumentStorageService documentStorageService)
        {
            _documentStorageService = documentStorageService;
        }

        [Function("UploadDocument")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post",
                Route = "documents/upload")] HttpRequestData req)
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

                return badRequest;
            }

            string fileName = fileNameValues.First();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("File name cannot be empty.");

                return badRequest;
            }

            //Upload the request body directly to Blob Storage
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(
                req.Body,
                overwrite: true);

            var response = req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                message = "Document uploaded successfully.",
                fileName = fileName
            });

            return response;
        }
    }
}
