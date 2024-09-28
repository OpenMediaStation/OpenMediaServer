using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IInventoryService
{
    public Task AddItem<T>(T item) where T : InventoryItem;
    public Task AddItems(IEnumerable<InventoryItem> items);
    public Task CreateFromPaths(IEnumerable<string> paths);
    public IEnumerable<string> ListCategories();
    public Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem;
    public Task<T?> GetItem<T>(Guid id, string category) where T : InventoryItem;
    public Task<T?> GetItemByName<T>(string name, string category) where T : InventoryItem;
}
