using System.Text.Json;
using System.Text.RegularExpressions;
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

        var pathRegex = new Regex(
            @"(?<category>(Movies)|(Shows)|\w+?)/.*?(?<folderTitle>[ \w.]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<seasonFolder>(([sS]taffel ?)|([Ss]eason ?))\d+)?/?(?<title>([ \w\.]+?))((\(|\.)(?<year>\d{4})(\)|\.?))?(([sS](?<season>\d*))([eE](?<episode>\d+)))?((-|\.)(?<fileInfo>[\w\.]*?))?\.(?<extension>\S{3,4})$", 
            RegexOptions.Compiled
            );
        
        foreach (var path in paths)
        {
            var match = pathRegex.Match(path.Replace(Globals.MediaFolder, string.Empty));
            if(!match.Success)
            {
                _logger.LogError("Invalid path: {Path}", path);
                continue;
            }
            var groups = match.Groups;

            switch (groups["category"].Value)
            {
                case "Movies":
                    {
                        _logger.LogDebug("Movie detected");

                        var movie = new Movie()
                        {
                            Id = Guid.NewGuid(),
                            Path = path,
                            Title = groups["title"].Value
                        };
                        
                        movie.Metadata = await _metadataAPI.GetMetadata(
                            name: movie.Title, 
                            apiKey: _configuration.GetValue<string>("OpenMediaServer:OMDbKey"),
                            year: groups.TryGetValue("year", out var movieTitleYear) ? 
                                movieTitleYear.Value : groups.TryGetValue("folderYear", out var movieFolderYear) ?
                                    movieFolderYear.Value : null
                            );

                        await AddItem(movie);
                        break;
                    }

                case "Shows":
                    {
                        _logger.LogDebug("Show detected");

                        // Show
                        var show = await GetItemByName<Show>(groups["folderTitle"].Value, "Show");

                        if (show == null)
                        {
                            show = new Show(); ;
                            show.Id = Guid.NewGuid();

                            show.Title = groups["folderTitle"].Value;
                            show.Path = Path.Combine(Globals.MediaFolder, "Shows", show.Title);
                            show.Metadata = await _metadataAPI.GetMetadata(
                                name: show.Title, 
                                apiKey: _configuration.GetValue<string>("OpenMediaServer:OMDbKey"),
                                year: groups["yearFolder"].Value,
                                type: "series"
                                );

                            await AddItem(show);
                        }

                        // Season
                        var season = await GetItemByName<Season>(groups["season"].Value, "Season");

                        if (season == null)
                        {
                            season = new Season();
                            season.Id = Guid.NewGuid();

                            season.ShowId = show.Id;
                            season.Title = groups["season"].Value;
                            season.SeasonNr = int.TryParse(groups["season"].Value, out var seasonNr) ? seasonNr : 0;
                            season.Path = Directory.GetParent(path)?.FullName ?? Directory.GetCurrentDirectory();
                            //Path.Combine(Globals.MediaFolder, "Shows", show.Title, groups["seasonFolder"].Value);

                            await AddItem(season);
                        }

                        // Episode
                        var title = groups["title"].Value;
                        var episode = await GetItemByName<Episode>(title, "Episode");

                        if (episode == null)
                        {
                            episode = new Episode();
                            episode.Id = Guid.NewGuid();

                            episode.SeasonId = season.Id;
                            episode.Title = groups["title"].Value;
                            episode.Path = path;
                            episode.EpisodeNr = int.TryParse(groups["episode"].Value, out var episodeNr) ? episodeNr : 0;

                            await AddItem(episode);
                        }

                        break;
                    }

                default:
                    _logger.LogWarning("Unknown category {CategoryName}", groups["category"].Value);
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
