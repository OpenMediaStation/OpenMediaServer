using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IBinService
{
    Task AddItem<T>(T item) where T : InventoryItem;
    Task RemoveById<T>(T item) where T : InventoryItem;
    Task<T?> GetItem<T>(string title, string category) where T : InventoryItem;
}
