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

    public IEnumerable<string> ListCategories()
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

    public async Task<T?> GetItemByName<T>(string? name, string category) where T : InventoryItem
    {
        _logger.LogTrace("Getting item by name");

        if (name == null)
        {
            return null;
        }

        try
        {
            var text = await File.ReadAllTextAsync(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");
            var items = JsonSerializer.Deserialize<IEnumerable<T>>(text);
            var possibleItems = items?.Where(i => i.Title == name);

            if (possibleItems == null || possibleItems.Count() != 1)
            {
                _logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Count());
                throw new ArgumentException("No id found in category");
            }

            return possibleItems.First();
        }
        catch (DirectoryNotFoundException dirEx)
        {
            _logger.LogWarning(dirEx, "Directory could not be found");

            return null;
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

    public async Task CreateFromPaths(IEnumerable<string> paths)
    {
        _logger.LogTrace("Creating from path");

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

            switch (parts[0])
            {
                case "Movies":
                    {
                        _logger.LogDebug("Movie detected");

                        var movie = new Movie();
                        movie.Id = Guid.NewGuid();
                        movie.Path = path;
                        movie.Title = parts.LastOrDefault()?.Split(".").FirstOrDefault();
                        movie.Metadata = await _metadataAPI.GetMetadata(movie.Title, _configuration.GetValue<string>("OpenMediaServer:OMDbKey"));

                        await AddItem(movie);
                        break;
                    }

                case "Shows":
                    {
                        _logger.LogDebug("Show detected");

                        // Show
                        var show = await GetItemByName<Show>(parts[1], "Show");

                        if (show == null)
                        {
                            show = new Show(); ;
                            show.Id = Guid.NewGuid();

                            show.Title = parts[1];
                            show.Path = Path.Combine(Globals.MediaFolder, "Shows", show.Title);
                            show.Metadata = await _metadataAPI.GetMetadata(show.Title, _configuration.GetValue<string>("OpenMediaServer:OMDbKey"));

                            await AddItem(show);
                        }

                        // Season
                        var season = await GetItemByName<Season>(parts[2], "Season");

                        if (season == null)
                        {
                            season = new Season();
                            season.Id = Guid.NewGuid();

                            season.ShowId = show.Id;
                            season.Title = parts[2];
                            season.Path = Path.Combine(Globals.MediaFolder, "Shows", show.Title, season.Title);

                            await AddItem(season);
                        }

                        // Episode
                        var title = parts.LastOrDefault()?.Split(".").FirstOrDefault();
                        var episode = await GetItemByName<Episode>(title, "Episode");

                        if (episode == null)
                        {
                            episode = new Episode();
                            episode.Id = Guid.NewGuid();

                            episode.SeasonId = season.Id;
                            episode.Title = parts.LastOrDefault()?.Split(".").FirstOrDefault();
                            episode.Path = path;

                            await AddItem(episode);
                        }

                        break;
                    }

                default:
                    _logger.LogWarning("Unknown category {CategoryName}", parts[0]);
                    break;
            }
        }
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
        await CreateFilesIfNeeded(item);

        var items = await _storageRepository.ReadObject<IEnumerable<T>>(GetPath(item));
        if (!items.Any(i => i.Title == item.Title))
        {
            items = items.Append(item);
            await _storageRepository.WriteObject(GetPath(item), items);
        }
    }

    public async Task Update<T>(T item) where T : InventoryItem
    {
        await CreateFilesIfNeeded(item);

        var items = await _storageRepository.ReadObject<List<T>>(GetPath(item));

        var existingItem = items.FirstOrDefault(i => i.Title == item.Title);

        if (existingItem != null)
        {
            items.Remove(existingItem);
        }

        items.Add(item);

        await _storageRepository.WriteObject(GetPath(item), items);
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
