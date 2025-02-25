using System;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Interfaces.Services;

public interface IAddonService
{
    IEnumerable<InventoryItemAddon> DiscoverAddons(string path);
    Task<Stream?> DownloadAddon(Guid inventoryItemId, string category, Guid addonId);
    IEnumerable<string> GetPaths(string path, SearchOption searchOption = SearchOption.AllDirectories);
}
