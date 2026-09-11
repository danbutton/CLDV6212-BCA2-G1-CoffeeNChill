using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Functions.Models;

/// <summary>
/// Table Storage representation of a single CoffeeNChill menu item.
///
/// PartitionKey = Category ("Hot Drinks"), RowKey = SKU ("COF-001").
/// Partitioning by category makes a category lookup a single-partition query,
/// which is the cheapest read shape Table Storage offers.
/// </summary>
/// <remarks>OWNER: A — agreed in the contract sprint. Announce before changing.</remarks>
public class MenuItemEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;  // Category
    public string RowKey { get; set; } = string.Empty;        // Item SKU

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Azure Table Storage has no native decimal type — decimal throws on
    // serialisation. double is the supported numeric type for currency here.
    public double Price { get; set; }

    public bool IsAvailable { get; set; }

    // Managed by the service, required by ITableEntity.
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
