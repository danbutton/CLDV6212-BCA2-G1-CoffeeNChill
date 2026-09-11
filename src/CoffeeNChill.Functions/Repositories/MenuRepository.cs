using CoffeeNChill.Functions.Models;

namespace CoffeeNChill.Functions.Repositories;

/// <summary>
/// Azure Table Storage implementation of <see cref="IMenuRepository"/>.
/// </summary>
/// <remarks>
/// OWNER: A — STUB. Replace each NotImplementedException with a real
/// implementation. See issue [A] "implement MenuRepository CRUD".
/// </remarks>
public class MenuRepository : IMenuRepository
{
    // TODO(A): inject IConfiguration + ILogger, build a TableClient from
    //          TableStorageConnection, call CreateIfNotExists() here.

    public Task<MenuItemEntity?> GetAsync(string category, string sku, CancellationToken ct = default)
        => throw new NotImplementedException("MenuRepository.GetAsync — see issue [A]");

    public Task<IReadOnlyList<MenuItemEntity>> GetAllAsync(CancellationToken ct = default)
        => throw new NotImplementedException("MenuRepository.GetAllAsync — see issue [A]");

    public Task<IReadOnlyList<MenuItemEntity>> GetByCategoryAsync(string category, CancellationToken ct = default)
        => throw new NotImplementedException("MenuRepository.GetByCategoryAsync — see issue [A]");

    public Task<MenuItemEntity> CreateAsync(MenuItemEntity item, CancellationToken ct = default)
        => throw new NotImplementedException("MenuRepository.CreateAsync — see issue [A]");

    public Task<MenuItemEntity?> UpdateAsync(MenuItemEntity item, CancellationToken ct = default)
        => throw new NotImplementedException("MenuRepository.UpdateAsync — see issue [A]");

    public Task<bool> DeleteAsync(string category, string sku, CancellationToken ct = default)
        => throw new NotImplementedException("MenuRepository.DeleteAsync — see issue [A]");
}
