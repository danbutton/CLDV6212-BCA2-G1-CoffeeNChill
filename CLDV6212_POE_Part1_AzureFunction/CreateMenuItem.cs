using Azure.Data.Tables;
using CLDV6212_POE_Part1_AzureFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CLDV6212_POE_Part1_AzureFunction
{
    public class CreateMenuItem
    {
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menuitems")]
            HttpRequestData req)
        {
            var menuItem = await JsonSerializer.DeserializeAsync<MenuItems>(
                req.Body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var tableClient =
                new TableClient(connectionString, "MenuItems");

            await tableClient.CreateIfNotExistsAsync();

            menuItem!.PartitionKey = "Items";

            if (string.IsNullOrWhiteSpace(menuItem.RowKey))
            {
                menuItem.RowKey = Guid.NewGuid().ToString();
            }

            await tableClient.AddEntityAsync(menuItem);

            var response =
                req.CreateResponse(HttpStatusCode.Created);

            await response.WriteAsJsonAsync(menuItem);

            return response;
        }
    }
}
