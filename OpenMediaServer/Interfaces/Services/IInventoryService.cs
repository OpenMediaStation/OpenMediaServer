using System;
using System.Linq.Expressions;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IInventoryService
{
    Task AddItem<T>(T item) where T : InventoryItem;
    Task AddItems(IEnumerable<InventoryItem> items);
    IEnumerable<string> ListCategories();
    Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem;
    Task<T?> GetItem<T>(Guid id) where T : InventoryItem;
    Task<T?> GetItem<T>(string category, Expression<Func<T, bool>> filter) where T : InventoryItem;
    Task Update<T>(T item) where T : InventoryItem;
    Task Remove<T>(T item) where T : InventoryItem;
}
