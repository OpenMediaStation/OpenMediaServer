using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;
    private readonly IFileSystemRepository _storageRepository;

    public InventoryService(ILogger<InventoryService> logger, IFileSystemRepository storageRepository)
    {
        _logger = logger;
        _storageRepository = storageRepository;
    }

    public IEnumerable<string> ListCategories()
    {
        var files = _storageRepository.GetFiles(Path.Join(Globals.ConfigFolder, "inventory"));
        var fileNames = files.Select(i => i.Split("/").Last().Replace(".json", ""));

        return fileNames;
    }

    public async Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem
    {
        var items = await _storageRepository.ReadObject<IEnumerable<T>>(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");

        if (items != null)
        {
            return items;
        }
        else
        {
            _logger.LogWarning("Category could not be found");

            return null;
        }
    }

    public async Task<T?> GetItem<T>(Guid id, string category) where T : InventoryItem
    {
        var items = await _storageRepository.ReadObject<IEnumerable<T>>(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");

        var possibleItems = items?.Where(i => i.Id == id);

        if (possibleItems == null || possibleItems.Count() != 1)
        {
            _logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Count());
            _logger.LogWarning("Id could not be found in category");

            return null;
        }

        return possibleItems.FirstOrDefault();
    }

    public async Task<T?> GetItem<T>(string category, Func<T, bool> predicate) where T : InventoryItem
    {
        _logger.LogTrace("Getting item by name");

        var items = await _storageRepository.ReadObject<IEnumerable<T>>(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");

        var possibleItems = items?.Where(predicate);

        if (possibleItems == null || possibleItems.Count() != 1)
        {
            _logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Count());
            _logger.LogWarning("Id could not be found in category");

            return null;
        }

        return possibleItems.FirstOrDefault();
    }

    public async Task AddItems(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            await AddItem(item);
        }
    }

    public async Task AddItem<T>(T item) where T : InventoryItem
    {
        var items = await _storageRepository.ReadObject<IEnumerable<T>>(GetPath(item));

        items ??= [];

        if (!items.Any(i => i.Id == item.Id))
        {
            items = items.Append(item);
            await _storageRepository.WriteObject(GetPath(item), items);
        }
        else
        {
            throw new ArgumentException("Item already exists");
        }
    }

    public async Task AddOrUpdate<T>(T? item) where T : InventoryItem
    {
        if (item == null)
        {
            _logger.LogWarning("Tried to add empty item in inventory");
            return;
        }

        try
        {
            await AddItem(item);
        }
        catch (ArgumentException)
        {
            await UpdateByTitle(item);
        }
    }

    public async Task UpdateByTitle<T>(T item) where T : InventoryItem
    {
        var items = await _storageRepository.ReadObject<List<T>>(GetPath(item));

        items ??= [];

        var existingItem = items.FirstOrDefault(i => i.Title == item.Title);

        if (existingItem != null)
        {
            items.Remove(existingItem);
        }

        items.Add(item);

        await _storageRepository.WriteObject(GetPath(item), items);
    }

    public async Task UpdateById<T>(T item) where T : InventoryItem
    {
        var items = await _storageRepository.ReadObject<List<T>>(GetPath(item));

        items ??= [];

        var existingItem = items.FirstOrDefault(i => i.Id == item.Id);

        if (existingItem != null)
        {
            items.Remove(existingItem);
        }

        items.Add(item);

        await _storageRepository.WriteObject(GetPath(item), items);
    }

    public async Task RemoveById<T>(T item) where T : InventoryItem
    {
        var items = await _storageRepository.ReadObject<List<T>>(GetPath(item));

        items ??= [];

        var existingItem = items.FirstOrDefault(i => i.Id == item.Id);

        if (existingItem != null)
        {
            items.Remove(existingItem);
        }

        await _storageRepository.WriteObject(GetPath(item), items);
    }

    private string GetPath(InventoryItem item)
    {
        return Path.Join(Globals.ConfigFolder, "inventory", item.Category + ".json");
    }
}
