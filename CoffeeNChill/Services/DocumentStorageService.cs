using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Services
{    
        public class DocumentStorageService
        {

        /*Reference
        * Author:Microsoft Ignite
        * Title: BlobContainerClient Class
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobcontainerclient?view=azure-dotnet
        */

        private readonly BlobContainerClient _containerClient;

            public DocumentStorageService(IConfiguration configuration)
            {
                string connectionString = configuration["AzureWebJobsStorage"]
                    ?? throw new InvalidOperationException(
                        "AzureWebJobsStorage connection string is not configured.");

                //Connect to the staff-docs Blob Storage container
                BlobServiceClient blobServiceClient =
                    new BlobServiceClient(connectionString);

                _containerClient =
                    blobServiceClient.GetBlobContainerClient("staff-docs");

                //Create the container if it does not already exist
                _containerClient.CreateIfNotExists();
            }

            public BlobContainerClient GetContainerClient()
            {
                return _containerClient;
            }
        }
    }
