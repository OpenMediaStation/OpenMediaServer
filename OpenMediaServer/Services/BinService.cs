using System;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class BinService : IBinService
{
    private readonly ILogger<BinService> _logger;
    private readonly IFileSystemRepository _fileSystemRepository;

    public BinService(ILogger<BinService> logger, IFileSystemRepository fileSystemRepository)
    {
        _logger = logger;
        _fileSystemRepository = fileSystemRepository;
    }

    public async Task<T?> GetItem<T>(string title, string category) where T : InventoryItem
    {
        var items = await _fileSystemRepository.ReadObject<IEnumerable<T>>(GetPath(category));

        var possibleItems = items?.Where(i => i.Title == title);

        if (possibleItems == null || possibleItems.Count() != 1)
        {
            _logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Count());
            _logger.LogWarning("Id could not be found in category");

            return null;
        }

        return possibleItems.FirstOrDefault();
    }

    public async Task AddItem<T>(T item) where T : InventoryItem
    {
        var items = await _fileSystemRepository.ReadObject<IEnumerable<T>>(GetPath(item));

        items ??= [];

        if (!items.Any(i => i.Id == item.Id))
        {
            items = items.Append(item);
            await _fileSystemRepository.WriteObject(GetPath(item), items);
        }
        else
        {
            throw new ArgumentException("Item already exists");
        }
    }

    public async Task RemoveById<T>(T item) where T : InventoryItem
    {
        var items = await _fileSystemRepository.ReadObject<List<T>>(GetPath(item));

        items ??= [];

        var existingItem = items.FirstOrDefault(i => i.Id == item.Id);

        if (existingItem != null)
        {
            items.Remove(existingItem);
        }

        await _fileSystemRepository.WriteObject(GetPath(item), items);
    }

    private string GetPath(InventoryItem item)
    {
        return Path.Join(Globals.ConfigFolder, "bin", item.Category + ".json");
    }

    private string GetPath(string category)
    {
        return Path.Join(Globals.ConfigFolder, "bin", category + ".json");
    }
}
