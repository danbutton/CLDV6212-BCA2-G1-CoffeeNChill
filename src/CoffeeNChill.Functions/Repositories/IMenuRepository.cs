using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Repositories;

/// <summary>
/// Storage contract for menu items. Functions depend on this interface, never
/// on TableClient directly — that is the "separation of storage concerns" the
/// marking rubric asks for, and it is what lets B build against a test double
/// before A's real implementation lands.
/// </summary>
/// <remarks>CONTRACT — agreed by the group. Changing a signature breaks other
/// members' work, so announce in the group chat first.</remarks>
public interface IMenuRepository
{
    Task<MenuItemEntity?> GetAsync(string category, string sku, CancellationToken ct = default);
    Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MenuItemEntity>> GetByCategoryAsync(string category, CancellationToken ct = default);
    Task<MenuItemEntity> CreateAsync(MenuItemEntity item, CancellationToken ct = default);
    Task<MenuItemEntity?> UpdateAsync(MenuItemEntity item, CancellationToken ct = default);
    Task<bool> DeleteAsync(string category, string sku, CancellationToken ct = default);
}
