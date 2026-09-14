using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions
{    
        public class DeleteMenuItem
        {

        /*Reference
        * Author:Microsoft Ignite
        * Title: TableClient.DeleteEntityAsync Method
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient.deleteentityasync?view=azure-dotnet
        */

        //Service used to delete menu items from Azure Table Storage
        private readonly MenuItemService _menuItemService;

            public DeleteMenuItem(MenuItemService menuItemService)
            {
                _menuItemService = menuItemService;
            }

            //HTTP DELETE endpoint: /api/menu/{category}/{id}
            [Function("DeleteMenuItem")]
            public async Task<HttpResponseData> Run(
                [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "delete",
                Route = "menu/{category}/{id}")]
            HttpRequestData req,
                string category,
                string id)
            {
                try
                {
                    //Check if category and item ID were provided
                    if (string.IsNullOrWhiteSpace(category) ||
                        string.IsNullOrWhiteSpace(id))
                    {
                        var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badRequest.WriteStringAsync(
                            "Category and item ID are required.");
                        return badRequest;
                    }

                    //Check if menu item exists before deleting it
                    var existingItem = await _menuItemService
                        .GetMenuItemAsync(category, id);

                    //Return 404 if the menu item does not exist
                    if (existingItem == null)
                    {
                        var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFound.WriteStringAsync(
                            $"Menu item '{id}' was not found in category '{category}'.");
                        return notFound;
                    }

                    //Delete menu item from Azure Table Storage
                    await _menuItemService.DeleteMenuItemAsync(category, id);

                    //Return 204 No Content when the item is successfully deleted
                    var response = req.CreateResponse(HttpStatusCode.NoContent);

                    return response;
                }
                catch (Azure.RequestFailedException ex)
                {
                    //Return 404 if the menu item could not be found
                    if (ex.Status == 404)
                    {
                        var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFound.WriteStringAsync(
                            $"Menu item '{id}' was not found.");
                        return notFound;
                    }

                    //Return 500 for other Azure Table Storage errors
                    var errorResponse = req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                    await errorResponse.WriteStringAsync(
                        $"An error occurred while deleting the menu item: {ex.Message}");

                    return errorResponse;
                }
                catch (Exception ex)
                {
                    //Return 500 if another unexpected error occurs
                    var errorResponse = req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                    await errorResponse.WriteStringAsync(
                        $"An error occurred while deleting the menu item: {ex.Message}");

                    return errorResponse;
                }
            }
        }
    }