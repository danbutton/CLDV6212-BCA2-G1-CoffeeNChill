namespace CoffeeNChill.Functions.DTOs;

/// <summary>
/// Partial update for PUT /api/menu/{category}/{id}.
/// Nullable properties mean "not supplied — leave unchanged".
/// </summary>
/// <remarks>OWNER: A</remarks>
public class UpdateMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Price { get; set; }
    public bool? IsAvailable { get; set; }
}
