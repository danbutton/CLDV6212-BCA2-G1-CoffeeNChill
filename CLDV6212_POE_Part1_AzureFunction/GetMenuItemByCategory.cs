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
    public class GetMenuItemsByCategory
    {
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menuitems/category/{category}")]
        HttpRequestData req,
            string category)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            var tableClient =
                new TableClient(connectionString, "MenuItems");

            var menuItems = new List<MenuItems>();

            await foreach (var menuItem in tableClient.QueryAsync<MenuItems>())
            {
                string fieldValue = category.ToLower() switch
                {
                    "colddrinks" => menuItem.ColdDrinks,
                    "hotdrinks" => menuItem.HotDrinks,
                    "pastries" => menuItem.Pastries,
                    "sandwiches" => menuItem.Sandwiches,
                    "bakedgoods" => menuItem.BakedGoods,
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(fieldValue))
                {
                    menuItems.Add(menuItem);
                }
            }

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(menuItems);

            return response;
        }
    }
}
