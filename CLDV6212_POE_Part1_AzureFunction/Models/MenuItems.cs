using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace CLDV6212_POE_Part1_AzureFunction.Models
{
    public class MenuItems : ITableEntity
    {
        public string PartitionKey { get; set; } = "Items";

        public string RowKey { get; set; } = string.Empty;

        public string ColdDrinks { get; set; } = string.Empty;

        public string HotDrinks { get; set; } = string.Empty;

        public string Pastries { get; set; } = string.Empty;

        public string Sandwiches { get; set; } = string.Empty;

        public string BakedGoods { get; set; } = string.Empty;

        public double Price { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}
