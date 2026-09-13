using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Services
{    
        public class DocumentStorageService
        {
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
