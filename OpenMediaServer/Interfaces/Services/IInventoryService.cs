using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IInventoryService
{
    Task AddItem<T>(T item) where T : InventoryItem;
    Task AddItems(IEnumerable<InventoryItem> items);
    IEnumerable<string> ListCategories();
    Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem;
    Task<T?> GetItem<T>(Guid id, string category) where T : InventoryItem;
    Task<T?> GetItem<T>(string category, Func<T, bool> predicate) where T : InventoryItem;
    Task UpdateByTitle<T>(T item) where T : InventoryItem;
    Task UpdateById<T>(T item) where T : InventoryItem;
    Task RemoveById<T>(T item) where T : InventoryItem;
}
