using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CLDV6212_POE_Part1_AzureFunction
{
    public class DeleteMenuItems
    {
        [Function("DeleteMenuItems")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "delete",
            Route = "menuitems/{id}")]
        HttpRequestData req,
        string id)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var tableClient =
                new TableClient(connectionString, "MenuItems");

            await tableClient.DeleteEntityAsync("Items", id);

            var response =
                req.CreateResponse(HttpStatusCode.NoContent);

            return response;
        }
    }
}
