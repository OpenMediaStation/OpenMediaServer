using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class InventoryService : IInventoryService
{
    private readonly ILogger<InventoryService> _logger;
    private readonly IFileSystemRepository _storageRepository;
    private readonly IMetadataService _metadataService;
    private readonly IFileInfoService _fileInfoService;

    public InventoryService(ILogger<InventoryService> logger, IFileSystemRepository storageRepository, IMetadataService metadataService, IFileInfoService fileInfoService)
    {
        _logger = logger;
        _storageRepository = storageRepository;
        _metadataService = metadataService;
        _fileInfoService = fileInfoService;
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

    public async Task CreateFromPaths(IEnumerable<string> paths)
    {
        _logger.LogTrace("Creating from path");

        var pathRegex = new Regex
        (
            pattern: @"(?<category>(Movies)|(Shows)|\w+?)/.*?(?<folderTitle>[ \w.]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<seasonFolder>(([sS]taffel ?)|([Ss]eason ?))\d+)?/?(?<title>([ \w\.]+?))((\(|\.)(?<year>\d{4})(\)|\.?))?(([sS](?<season>\d*))([eE](?<episode>\d+)))?((-|\.)(?<fileInfo>[\w\.]*?))?\.(?<extension>\S{3,4})$",
            options: RegexOptions.Compiled
        );

        foreach (var path in paths)
        {
            var match = pathRegex.Match(path.Replace(Globals.MediaFolder, string.Empty));
            if (!match.Success)
            {
                _logger.LogWarning("Invalid path: {Path}", path);
                continue;
            }
            var groups = match.Groups;
            var category = groups["category"].Value;
            var folderTitle = groups["folderTitle"].Value;
            var title = groups["title"].Value;

            switch (category)
            {
                case "Movies":
                    {
                        _logger.LogDebug("Movie detected");

                        var movies = await ListItems<Movie>("Movie");
                        var existingMovie = movies?.Where(i => i.Versions?.Any(i => i.Path == path) ?? false).FirstOrDefault();

                        string? folderPath = null;

                        if (!string.IsNullOrEmpty(folderTitle))
                        {
                            folderPath = Path.Combine(Globals.MediaFolder, category, folderTitle);
                            existingMovie = movies?.Where(i => i.FolderPath == folderPath).FirstOrDefault();
                        }

                        if (existingMovie != null)
                        {
                            if (!string.IsNullOrEmpty(folderTitle))
                            {
                                var version = new InventoryItemVersion
                                {
                                    Id = Guid.NewGuid(),
                                    Path = path,
                                };

                                if (existingMovie.Versions?.Any(i => i.Path == path) ?? false)
                                {
                                    continue;
                                }

                                // Do this after the path check because a file info will be created
                                version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, category))?.Id;

                                existingMovie.Versions = existingMovie.Versions?.Append(version);

                                await Update(existingMovie);
                            }

                            continue;
                        }

                        var versionId = Guid.NewGuid();
                        var movie = new Movie()
                        {
                            Id = Guid.NewGuid(),
                            Versions =
                            [
                                new()
                                {
                                    Id = versionId,
                                    Path = path,
                                    FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, category))?.Id
                                }
                            ],
                            Title = title,
                            FolderPath = folderPath
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
                        var showPath = Path.Combine(Globals.MediaFolder, "Shows", folderTitle);
                        var show = await GetItem<Show>("Show", i => i.FolderPath == showPath);

                        if (show == null)
                        {
                            show = new Show
                            {
                                Id = Guid.NewGuid(),
                                Title = folderTitle,
                            };

                            show.FolderPath = Path.Combine(Globals.MediaFolder, "Shows", folderTitle);

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
                        var season = await GetItem<Season>("Season", i => i.FolderPath == Directory.GetParent(path)?.FullName);

                        if (season == null)
                        {
                            season = new Season
                            {
                                Id = Guid.NewGuid(),

                                ShowId = show.Id,
                                Title = groups["seasonFolder"].Value,
                                SeasonNr = int.TryParse(groups["season"].Value, out var seasonNr) ? seasonNr : null,
                                FolderPath = Directory.GetParent(path)?.FullName ?? Directory.GetCurrentDirectory()
                            };

                            await AddItem(season);
                        }

                        // Episode
                        var episode = await GetItem<Episode>("Episode", predicate: i => i.Versions?.Any(i => i.Path == path) ?? false);

                        if (episode == null)
                        {
                            var versionId = Guid.NewGuid();
                            episode = new Episode
                            {
                                Id = Guid.NewGuid(),

                                SeasonId = season.Id,
                                Title = title,
                                Versions =
                                [
                                    new()
                                    {
                                        Id = versionId,
                                        Path = path,
                                        FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Episode"))?.Id
                                    }
                                ],
                                EpisodeNr = int.TryParse(groups["episode"].Value, out var episodeNr) ? episodeNr : null,
                                SeasonNr = season.SeasonNr
                            };

                            var metadata = await _metadataService.CreateNewMetadata
                            (
                                parentId: episode.Id,
                                title: episode.Title,
                                year: groups["yearFolder"].Value,
                                category: episode.Category,
                                episode: episode.EpisodeNr,
                                season: episode.SeasonNr
                            );

                            episode.MetadataId = metadata?.Id;

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
