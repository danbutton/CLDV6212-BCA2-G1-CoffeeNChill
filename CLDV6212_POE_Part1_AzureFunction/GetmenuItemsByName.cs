using Azure.Data.Tables;
using CLDV6212_POE_Part1_AzureFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CLDV6212_POE_Part1_AzureFunction
{
    public class GetMenuItemsByName
    {
        [Function("GetMenuItemsByName")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menuitems/search/{name}")]
        HttpRequestData req,
            string name)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var tableClient =
                new TableClient(connectionString, "MenuItems");

            var menuItems = new List<MenuItems>();

            await foreach (var menuItem in tableClient.QueryAsync<MenuItems>(
                m => m.RowKey == name))
            {
                menuItems.Add(menuItem);
            }

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(menuItems);

            return response;
        }
    }
}
