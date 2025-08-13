using System;
using System.Linq.Expressions;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IInventoryService
{
    Task AddItem(InventoryItem item);
    Task AddItems(IEnumerable<InventoryItem> items);
    IEnumerable<string> ListCategories();
    Task<IEnumerable<InventoryItem>?> ListItems(string category);
    Task<InventoryItem?> GetItem(Guid? id);
    Task<InventoryItem?> GetItem(string category, Expression<Func<InventoryItem, bool>> filter);
    Task UpdateOrInsert(InventoryItem item);
    Task Remove(InventoryItem item);
}
