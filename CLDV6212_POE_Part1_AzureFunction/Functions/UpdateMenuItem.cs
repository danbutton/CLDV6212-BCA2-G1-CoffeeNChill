using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace CoffeeNChill.Functions
{    
        public class UpdateMenuItem
        {
            //Service used to retrieve and update menu items in Azure Table Storage
            private readonly MenuItemService _menuItemService;

            public UpdateMenuItem(MenuItemService menuItemService)
            {
                _menuItemService = menuItemService;
            }

            //HTTP PUT endpoint: /api/menu/{category}/{id}
            [Function("UpdateMenuItem")]
            public async Task<HttpResponseData> Run(
                [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "put",
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

                    //Find the existing menu item before updating it
                    var existingItem = await _menuItemService
                        .GetMenuItemAsync(category, id);

                    //Return 404 if menu item does not exist
                    if (existingItem == null)
                    {
                        var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFound.WriteStringAsync(
                            $"Menu item '{id}' was not found in category '{category}'.");
                        return notFound;
                    }

                    //Read updated menu item from the request body
                    var updatedItem = await JsonSerializer.DeserializeAsync<MenuItem>(
                        req.Body,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    //Check that valid menu item data was provided
                    if (updatedItem == null)
                    {
                        var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badRequest.WriteStringAsync(
                            "Invalid menu item data.");
                        return badRequest;
                    }

                    //Validate the required fields and price
                    if (string.IsNullOrWhiteSpace(updatedItem.Name) ||
                        string.IsNullOrWhiteSpace(updatedItem.Description) ||
                        updatedItem.Price < 0)
                    {
                        var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badRequest.WriteStringAsync(
                            "Name and Description are required, and Price cannot be negative.");
                        return badRequest;
                    }

                    //Keep the original Azure Table keys and ETag
                    updatedItem.PartitionKey = existingItem.PartitionKey;
                    updatedItem.RowKey = existingItem.RowKey;
                    updatedItem.ETag = existingItem.ETag;

                    //Update the menu item in Azure Table Storage
                    await _menuItemService.UpdateMenuItemAsync(updatedItem);

                    //Return the updated menu item with a 200 OK response
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(updatedItem);

                    return response;
                }
                catch (Azure.RequestFailedException ex)
                {
                    //Return 404 if the item could not be found during the update
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
                        $"An error occurred while updating the menu item: {ex.Message}");

                    return errorResponse;
                }
                catch (Exception ex)
                {
                    //Return 500 if another unexpected error occurs
                    var errorResponse = req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                    await errorResponse.WriteStringAsync(
                        $"An error occurred while updating the menu item: {ex.Message}");

                    return errorResponse;
                }
            }
        }
    }