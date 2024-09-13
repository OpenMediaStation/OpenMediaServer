using System;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;
    private readonly IStorageRepository _storageRepository;

    public InventoryService(ILogger<InventoryService> logger, IStorageRepository storageRepository)
    {
        _logger = logger;
        _storageRepository = storageRepository;
    }

    public void CreateFromPaths(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var temp = path.Replace(Globals.DataFolder, "");

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
                movie.Title = parts[1];

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

        var movies = await _storageRepository.ReadObject<IEnumerable<T>>(GetPath(item));
        if (!movies.Any(i => i.Title == item.Title))
        {
            movies = movies.Append(item);
            await _storageRepository.WriteObject(GetPath(item), movies);
        }
    }

    private async Task CreateFilesIfNeeded(InventoryItem item)
    {
        var path = GetPath(item);
        if (!File.Exists(path))
        {
            using var stream = File.Create(path);
            TextWriter textWriter = new StreamWriter(stream);
            await textWriter.WriteAsync("[]");
            textWriter.Close();
        }
    }

    private string GetPath(InventoryItem item)
    {
        return Path.Join(Globals.ConfigFolder, item.Category + ".json");
    }
}
