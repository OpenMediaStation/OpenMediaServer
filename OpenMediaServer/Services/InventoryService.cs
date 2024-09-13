using System;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(ILogger<InventoryService> logger)
    {
        _logger = logger;
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

    public void AddItem<T>(T item) where T : InventoryItem
    {
        CreateFilesIfNeeded(item);

        
    }

    private void CreateFilesIfNeeded(InventoryItem item)
    {
        string path = Path.Join(Globals.ConfigFolder, item.Category + ".json");
        if (!File.Exists(path))
        {
            File.Create(path);
        }
    }
}
