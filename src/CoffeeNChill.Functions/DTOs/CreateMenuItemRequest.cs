namespace CoffeeNChill.Functions.DTOs;

/// <summary>
/// Inbound payload for POST /api/menu.
/// Deliberately does not expose PartitionKey/RowKey/ETag — clients speak in
/// domain terms (category, sku), not storage terms.
/// </summary>
/// <remarks>OWNER: A</remarks>
public class CreateMenuItemRequest
{
    public string Category { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}
