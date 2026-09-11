using CoffeeNChill.Functions.DTOs;

namespace CoffeeNChill.Functions.Validation;

/// <summary>
/// Validates inbound menu payloads, returning every failure at once so the
/// client gets one actionable 400 rather than a game of whack-a-mole.
/// </summary>
/// <remarks>OWNER: A — STUB. See issue [A] "implement MenuItemValidator".</remarks>
public static class MenuItemValidator
{
    public static readonly string[] AllowedCategories =
        { "Hot Drinks", "Cold Drinks", "Pastries", "Sandwiches" };

    public static List<string> Validate(CreateMenuItemRequest? r)
    {
        // TODO(A): category in allowlist; SKU non-empty and free of / \ # ?;
        //          name required max 100; description max 500;
        //          price > 0 and <= 10000.
        throw new NotImplementedException("MenuItemValidator.Validate — see issue [A]");
    }
}
