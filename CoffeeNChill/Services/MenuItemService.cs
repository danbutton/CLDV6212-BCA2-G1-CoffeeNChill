using Azure.Data.Tables;
using CoffeeNChill.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeNChill.Services
{
    public class MenuItemService
    {
        private readonly TableClient _tableClient;

        public MenuItemService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage connection string is not configured.");

            _tableClient = new TableClient(connectionString, "MenuItems");

            // Create the table if it doesn't already exist
            _tableClient.CreateIfNotExists();
        }

        public async Task<MenuItem> CreateMenuItemAsync(MenuItem menuItem)
        {
            await _tableClient.AddEntityAsync(menuItem);
            return menuItem;
        }

        public async Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            var menuItems = new List<MenuItem>();

            await foreach (MenuItem item in _tableClient.QueryAsync<MenuItem>())
            {
                menuItems.Add(item);
            }

            return menuItems;
        }

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
                return null;
            }
        }

        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            await _tableClient.UpdateEntityAsync(
                menuItem,
                menuItem.ETag,
                TableUpdateMode.Replace);
        }

        public async Task DeleteMenuItemAsync(
            string category,
            string id)
        {
            await _tableClient.DeleteEntityAsync(category, id);
        }
    }
}
