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
    public class UpdateMenuItems
    {
        [Function("UpdateMenuItems")]
        public async Task<HttpResponseData> Run(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "put",
            Route = "menuitems/{id}")]
        HttpRequestData req,
        string id)
        {
            var updatedMenuItem = await JsonSerializer.DeserializeAsync<MenuItems>(
                req.Body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var tableClient =
                new TableClient(connectionString, "MenuItems");

            var existingMenuItem =
                await tableClient.GetEntityAsync<MenuItems>("Items", id);

            existingMenuItem.Value.ColdDrinks = updatedMenuItem!.ColdDrinks;
            existingMenuItem.Value.HotDrinks = updatedMenuItem.HotDrinks;
            existingMenuItem.Value.Pastries = updatedMenuItem.Pastries;
            existingMenuItem.Value.Sandwiches = updatedMenuItem.Sandwiches;
            existingMenuItem.Value.BakedGoods = updatedMenuItem.BakedGoods;
            existingMenuItem.Value.Price = updatedMenuItem.Price;

            await tableClient.UpdateEntityAsync(
                existingMenuItem.Value,
                existingMenuItem.Value.ETag);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(existingMenuItem.Value);

            return response;
        }
    }
}
