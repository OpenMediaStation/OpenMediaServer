using System;
using System.Linq.Expressions;
using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Interfaces.Services;

public interface IAddonService
{
    IEnumerable<InventoryItemAddon> DiscoverAddons(string path);
    Task<Stream?> DownloadAddon(Guid inventoryItemId, string category, Guid addonId);
    IEnumerable<string> GetPaths(string path, SearchOption searchOption = SearchOption.AllDirectories);
    Task<IEnumerable<InventoryItemAddon>?> ListItems(Expression<Func<InventoryItemAddon, bool>>? filter = null);
    Task DeleteAddon(Guid addonId);
    Task UpdateOrInsert(InventoryItemAddon item);
}
