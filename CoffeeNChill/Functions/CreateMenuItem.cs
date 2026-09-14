using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions
{
    public class CreateMenuItem
    {

        /*Reference
        * Author:Microsoft Ignite
        * Title: Azure Functions HTTP trigger
        * Link: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-http-webhook-trigger?tabs=python-v2%2Cisolated-process%2Cnodejs-v4%2Cfunctionsv2&pivots=programming-language-csharp
        */

        /*Reference
        * Author:Microsoft Ignite
        * Title: TableClient.AddEntityAsync<T>(T, CancellationToken) Method
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.addentityasync?view=azure-dotnet
        */

        //Service used to save the menu item to Azure Table Storage
        private readonly MenuItemService _menuItemService;

        public CreateMenuItem(MenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        //HTTP POST endpoint to create a new menu item
        //endpoint: /api/menu
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req)
        {
            try
            {
                //Read the menu item in the request body
                var menuItem = await JsonSerializer.DeserializeAsync<MenuItem>(
                    req.Body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                //Check if menu item was actually provided
                if (menuItem == null)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync("Invalid menu item data.");
                    return badRequest;
                }

                //Validate required menu items
                if (string.IsNullOrWhiteSpace(menuItem.PartitionKey) ||
                    string.IsNullOrWhiteSpace(menuItem.RowKey) ||
                    string.IsNullOrWhiteSpace(menuItem.Name) ||
                    string.IsNullOrWhiteSpace(menuItem.Description) ||
                    menuItem.Price < 0)
                {
                    var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequest.WriteStringAsync(
                        "PartitionKey, RowKey, Name, and Description are required, and Price cannot be negative.");
                    return badRequest;
                }

                //Save menu item to Azure Table Storage
                await _menuItemService.CreateMenuItemAsync(menuItem);

                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(menuItem);

                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(
                    $"An error occurred while creating the menu item: {ex.Message}");

                return errorResponse;
            }
        }
    }
}
