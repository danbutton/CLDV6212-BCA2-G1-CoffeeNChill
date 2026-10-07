using Azure.Data.Tables;
using CoffeeNChill.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Services
{
    public class MenuItemService
    {

        /*Reference
        * Author:Microsoft Ignite
        * Title: TableClient Class
        * Link: https://learn.microsoft.com/en-us/dotnet/api/azure.data.tables.tableclient?view=azure-dotnet
        */


        //Client used to communicate with Azure Table Storage
        private readonly TableClient _tableClient;

        public MenuItemService(IConfiguration configuration)
        {
           //Get the connection string for Azure Table Storage, from application settings
            string connectionString = configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage connection string is not configured.");

            //Connect to MenuItems table
            _tableClient = new TableClient(connectionString, "MenuItems");

            //Create the table if it doesn't already exist
            _tableClient.CreateIfNotExists();
        }

        //Add new item to the Azure Table Storage
        public async Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem)
        {
            await _tableClient.AddEntityAsync(menuItem);
            return menuItem;
        }

        //Fetch all menu items from the table
        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            var menuItems = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>())
            {
                menuItems.Add(item);
            }

            return menuItems;
        }

        //Fetch menu items by category from the table
        public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(
            string category)
        {
            var menuItems = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>(
                item => item.PartitionKey == category))
            {
                menuItems.Add(item);
            }

            return menuItems;
        }

        //Retrieve a specific menu item by category and id from the table
        public async Task<MenuItem?> GetMenuItemAsync(
            string category,
            string id)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<MenuItem>(
                    category,
                    id);

                return response.Value;
            }
            catch (Azure.RequestFailedException ex)
                when (ex.Status == 404)
            {
                //If the item is not found, return null
                return null;
            }
        }

        //Update an existing menu item using its ETag
        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            await _tableClient.UpdateEntityAsync(
                menuItem,
                menuItem.ETag,
                TableUpdateMode.Replace);
        }

        //Delete a menu item from the table using its category and id
        public async Task DeleteMenuItemAsync(
            string category,
            string id)
        {
            await _tableClient.DeleteEntityAsync(category, id);
        }
    }
}
