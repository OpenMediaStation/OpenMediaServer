using System;
using System.Text.Json;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;
    private readonly IStorageRepository _storageRepository;
    private readonly IMetadataAPI _metadataAPI;
    private readonly IConfiguration _configuration;

    public InventoryService(ILogger<InventoryService> logger, IStorageRepository storageRepository, IMetadataAPI metadataAPI, IConfiguration configuration)
    {
        _logger = logger;
        _storageRepository = storageRepository;
        _metadataAPI = metadataAPI;
        _configuration = configuration;
    }

    public async Task<IEnumerable<string>> ListCategories()
    {
        var files = Directory.EnumerateFiles(Path.Join(Globals.ConfigFolder, "inventory"));
        var fileNames = files.Select(i => i.Split("/").Last().Replace(".json", ""));

        return fileNames;
    }

    public async Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem
    {
        try
        {
            var text = await File.ReadAllTextAsync(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");
            var items = JsonSerializer.Deserialize<IEnumerable<T>>(text);

            return items;
        }
        catch (FileNotFoundException fileEx)
        {
            _logger.LogWarning(fileEx, "Category could not be found");

            return null;
        }
    }

    public async Task<T?> GetItem<T>(Guid id, string category) where T : InventoryItem
    {
        try
        {
            var text = await File.ReadAllTextAsync(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");
            var items = JsonSerializer.Deserialize<IEnumerable<T>>(text);
            var possibleItems = items?.Where(i => i.Id == id);

            if (possibleItems == null || possibleItems.Count() != 1)
            {
                _logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Count());
                throw new ArgumentException("No id found in category");
            }

            return possibleItems.First();
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Id could not be found in category");

            return null;
        }
        catch (FileNotFoundException fileEx)
        {
            _logger.LogWarning(fileEx, "Category could not be found to retrieve id");

            return null;
        }
    }

    public async void CreateFromPaths(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var temp = path.Replace(Globals.MediaFolder, "");

            var parts = new List<string>();

            foreach (var part in temp.Split("/"))
            {
                if (!string.IsNullOrEmpty(part))
                {
                    parts.Add(part);
                }
            }

            if (parts[0] == "Movies")
            {
                var movie = new Movie();
                movie.Path = path;
                movie.Title = parts[1].Split(".").FirstOrDefault();
                movie.Metadata = await _metadataAPI.GetMetadata(movie.Title, _configuration.GetValue<string>("OpenMediaServer:OMDbKey"));

                AddItem(movie);
            }
            else
            {
                _logger.LogWarning("Unknown category {CategoryName}", parts[0]);
            }
        }
    }

    public void AddItems(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            AddItem(item);
        }
    }

    public async void AddItem<T>(T item) where T : InventoryItem
    {
        await CreateFilesIfNeeded(item);

        var items = await _storageRepository.ReadObject<IEnumerable<T>>(GetPath(item));
        if (!items.Any(i => i.Title == item.Title))
        {
            items = items.Append(item);
            await _storageRepository.WriteObject(GetPath(item), items);
        }
    }

    private async Task CreateFilesIfNeeded(InventoryItem item)
    {
        var path = GetPath(item);
        if (!File.Exists(path))
        {
            FileInfo file = new FileInfo(path);
            file.Directory?.Create();
            var stream = file.Create();
            TextWriter textWriter = new StreamWriter(stream);
            await textWriter.WriteAsync("[]");
            textWriter.Close();
        }
    }

    private string GetPath(InventoryItem item)
    {
        return Path.Join(Globals.ConfigFolder, "inventory", item.Category + ".json");
    }
}
