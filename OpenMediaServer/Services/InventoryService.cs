using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;
    private readonly IFileSystemRepository _storageRepository;
    private readonly IConfiguration _configuration;
    private readonly IMetadataService _metadataService;

    public InventoryService(ILogger<InventoryService> logger, IFileSystemRepository storageRepository, IConfiguration configuration, IMetadataService metadataService)
    {
        _logger = logger;
        _storageRepository = storageRepository;
        _configuration = configuration;
        _metadataService = metadataService;
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

        return possibleItems.First();
    }

    public async Task<T?> GetItemByName<T>(string? name, string category) where T : InventoryItem
    {
        _logger.LogTrace("Getting item by name");

        if (name == null)
        {
            return null;
        }

        var items = await _storageRepository.ReadObject<IEnumerable<T>>(Path.Combine(Globals.ConfigFolder, "inventory", category) + ".json");

        var possibleItems = items?.Where(i => i.Title == name);

        if (possibleItems == null || possibleItems.Count() != 1)
        {
            _logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Count());
            _logger.LogWarning("Id could not be found in category");

            return null;
        }

        return possibleItems.First();
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
            if (!match.Success)
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

                        var metadata = await _metadataService.CreateNewMetadata
                        (
                            parentId: movie.Id,
                            title: movie.Title,
                            category: movie.Category,
                            year: groups.TryGetValue("year", out var movieTitleYear) ?
                                    movieTitleYear.Value : groups.TryGetValue("folderYear", out var movieFolderYear) ?
                                    movieFolderYear.Value : null
                        );

                        movie.MetadataId = metadata?.Id;


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

                            var metadata = await _metadataService.CreateNewMetadata
                            (
                                parentId: show.Id,
                                title: show.Title,
                                year: groups["yearFolder"].Value,
                                category: show.Category
                            );

                            show.MetadataId = metadata?.Id;

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
                            season.SeasonNr = int.TryParse(groups["season"].Value, out var seasonNr) ? seasonNr : null;
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
                            episode.EpisodeNr = int.TryParse(groups["episode"].Value, out var episodeNr) ? episodeNr : null;

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
        var items = await _storageRepository.ReadObject<IEnumerable<T>>(GetPath(item));

        items ??= [];

        if (!items.Any(i => i.Title == item.Title))
        {
            items = items.Append(item);
            await _storageRepository.WriteObject(GetPath(item), items);
        }
    }

    public async Task Update<T>(T item) where T : InventoryItem
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

    private string GetPath(InventoryItem item)
    {
        return Path.Join(Globals.ConfigFolder, "inventory", item.Category + ".json");
    }
}
