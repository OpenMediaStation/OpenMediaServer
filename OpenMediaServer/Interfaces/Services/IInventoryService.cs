using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IInventoryService
{
    public void AddItem<T>(T item) where T : InventoryItem;
    public void AddItems(IEnumerable<InventoryItem> items);
    public void CreateFromPaths(IEnumerable<string> paths);
    public Task<IEnumerable<string>> ListCategories();
    public Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem;
    public Task<T?> GetItem<T>(Guid id, string category) where T : InventoryItem;
}
