using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.DTOs;

/// <summary>Outbound shape. Maps storage concepts back to business language.</summary>
/// <remarks>OWNER: A</remarks>
public class MenuItemResponse
{
    public string Category { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsAvailable { get; set; }
    public DateTimeOffset? LastModified { get; set; }

    public static MenuItemResponse FromEntity(MenuItemEntity e) => new()
    {
        Category     = e.PartitionKey,
        Sku          = e.RowKey,
        Name         = e.Name,
        Description  = e.Description,
        Price        = e.Price,
        IsAvailable  = e.IsAvailable,
        LastModified = e.Timestamp
    };
}
