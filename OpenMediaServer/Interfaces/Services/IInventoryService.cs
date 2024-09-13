using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IInventoryService
{
    public void AddItem<T>(T item) where T : InventoryItem;
    public void AddItems(IEnumerable<InventoryItem> items);
    public void CreateFromPaths(IEnumerable<string> paths);
}
