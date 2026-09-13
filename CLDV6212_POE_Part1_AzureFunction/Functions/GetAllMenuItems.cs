using CoffeeNChill.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace CoffeeNChill.Functions
{    
        public class GetAllMenuItems
        {
            //Service used to retrieve menu items from Azure Table Storage
            private readonly MenuItemService _menuItemService;

            public GetAllMenuItems(MenuItemService menuItemService)
            {
                _menuItemService = menuItemService;
            }

            //HTTP GET endpoint: /api/menu
            [Function("GetAllMenuItems")]
            public async Task<HttpResponseData> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequestData req)
            {
                try
                {
                    //Fetch all menu items from Azure Table Storage
                    var menuItems = await _menuItemService.GetAllMenuItemsAsync();

                //Return the menu items with a 200 OK response
                var response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteAsJsonAsync(menuItems);

                    return response;
                }
                catch (Exception ex)
                {
                    //Return 500 if an unexpected error occurs
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync(
                        $"An error occurred while retrieving menu items: {ex.Message}");

                    return errorResponse;
                }
            }
        }
    }
