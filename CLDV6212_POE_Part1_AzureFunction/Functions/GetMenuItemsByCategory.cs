using CoffeeNChill.Models;
using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace CoffeeNChill.Functions
{    
        public class GetMenuItemsByCategory
        {
            //Service used to retrieve menu items from Azure Table Storage
            private readonly MenuItemService _menuItemService;

            public GetMenuItemsByCategory(MenuItemService menuItemService)
            {
                _menuItemService = menuItemService;
            }

            //HTTP GET endpoint: /api/menu/category/{category}
            [Function("GetMenuItemsByCategory")]
            public async Task<HttpResponseData> Run(
                [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "menu/category/{category}")]
            HttpRequestData req,
                string category)
            {
                try
                {
                    //Check if category was provided in the URL
                    if (string.IsNullOrWhiteSpace(category))
                    {
                        var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                        await badRequest.WriteStringAsync("Category is required.");
                        return badRequest;
                    }

                    //Retrieve menu items belonging to the requested category
                    var menuItems = await _menuItemService
                        .GetMenuItemsByCategoryAsync(category);

                    //Return 404 if no menu items were found
                    if (menuItems.Count == 0)
                    {
                        var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                        await notFound.WriteStringAsync(
                            $"No menu items found for category '{category}'.");
                        return notFound;
                    }

                    //Return the matching menu items with a 200 OK response
                    var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(menuItems);

                    return response;
                }
                catch (Exception ex)
                {
                    //Return 500 if an unexpected error occurs
                    var errorResponse = req.CreateResponse(
                        HttpStatusCode.InternalServerError);

                    await errorResponse.WriteStringAsync(
                        $"An error occurred while retrieving menu items: {ex.Message}");

                    return errorResponse;
                }
            }
        }
    }