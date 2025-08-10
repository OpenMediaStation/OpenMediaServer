using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Discovery;
using OpenMediaServer.Models;
using TMDbLib.Objects.Movies;

namespace OpenMediaServer.Services.Discovery;

public class ContentDiscoveryService(ILogger<ContentDiscoveryService> logger, IDiscoveryShowService showService, IDiscoveryMovieService movieService, IDiscoveryBookService _bookService, IInventoryService _inventoryService, IBinService _binService, IAddonService _addonService, IFileInfoService _fileInfo, IDiscoveryAudiobookService _audiobookDiscoveryService, IVersionService versionService) : IContentDiscoveryService
{
    private readonly ILogger<ContentDiscoveryService> _logger = logger;
    private readonly IDiscoveryShowService _showService = showService;
    private readonly IDiscoveryMovieService _movieService = movieService;

    /// <summary>
    /// This method aims to do a full cleanup of the inventory. It first checks if there is something which was deleted and moves those 
    /// entries to the bin folder. After that a new scan can check if an entry like this has already existed before and reuse the same id. 
    /// </summary>
    /// <returns></returns>
    public async Task MoveToBinIfDeleted()
    {
        var paths = GetPaths(Globals.MediaFolder);

        var movies = await _inventoryService.ListItems("Movie");

        await HandleDelete(paths, movies);

        var books = await _inventoryService.ListItems("Book");

        await HandleDelete(paths, books);

        var audiobooks = await _inventoryService.ListItems("Audiobook");

        await HandleDelete(paths, books);

        var episodes = await _inventoryService.ListItems("Episode");

        await HandleDelete(paths, episodes);
    }

    /// <summary>
    /// Searches for new items but does not delete old ones. If something was deleted by Rescan beforehand and is now in bin it will be restored.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task ActiveScan(string path)
    {
        // This must be done before any scanning so that deleted items got moved to the bin
        await MoveToBinIfDeleted();

        IEnumerable<string> files = GetPaths(path);

        await CreateFromPaths(files);
    }

    public async Task CreateFromPaths(IEnumerable<string> paths)
    {
        _logger.LogTrace("Creating from path");

        foreach (var path in paths)
        {
            var category = path.Split('/')[2];

            switch (category)
            {
                case "Movies":
                    {
                        await _movieService.CreateMovie(path);

                        break;
                    }
                case "Shows":
                    {
                        await _showService.CreateShow(path);

                        break;
                    }
                case "Books":
                    {
                        await _bookService.CreateBook(path);

                        break;
                    }
                case "Audiobooks":
                    {
                        await _audiobookDiscoveryService.CreateAudiobook(path);

                        break;
                    }
                case "default":
                    {
                        _logger.LogWarning("Unknown category: {Category}", category);

                        break;
                    }
            }
        }
    }

    public void Watch(string path)
    {
        _logger.LogInformation("Watching file system");

        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = path;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Filter = "**";
        watcher.IncludeSubdirectories = true;
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.Created += new FileSystemEventHandler(OnChanged);
        watcher.Deleted += new FileSystemEventHandler(OnChanged);
        watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("FileSystem changed");

        ActiveScan(Globals.MediaFolder).Wait(); // TODO Might be problematic
    }

    private async Task UpdateShow(IEnumerable<InventoryItem>? seasons)
    {
        if (seasons != null)
        {
            foreach (var season in seasons)
            {
                // TODO
                
                // var show = await _inventoryService.GetItem<InventoryItem>(season.ShowId);

                // if (show != null)
                // {
                //     var seasonIds = show.SeasonIds?.ToList();
                //     seasonIds?.RemoveAll(i => i == season.Id);
                //
                //     show.SeasonIds = seasonIds;
                //
                //     await _inventoryService.Update(show);
                // }
                //
                // if (show != null && (!show?.SeasonIds?.Any() ?? true))
                // {
                //     await _binService.AddItem(show!);
                //     // await _inventoryService.RemoveById(show!);
                // }
            }
        }
    }

    private async Task UpdateSeason(IEnumerable<InventoryItem>? items)
    {
        if (items != null)
        {
            List<InventoryItem> seasons = [];

            foreach (var episode in items)
            {
                // TODO
                
                // var season = await _inventoryService.GetItem<InventoryItem>(episode.SeasonId);
                //
                // if (season != null)
                // {
                //     seasons.Add(season);
                // }
            }

            foreach (var episode in items)
            {
                var season = seasons.FirstOrDefault(i => i.Id == episode.SeasonId);

                // TODO
                // if (season != null)
                // {
                //     var episodeIds = season.EpisodeIds?.ToList();
                //     episodeIds?.RemoveAll(i => i == episode.Id);
                //
                //     season.EpisodeIds = episodeIds;
                //
                //     await _inventoryService.Update(season);
                // }
                //
                // if (season != null && (!season?.EpisodeIds?.Any() ?? true))
                // {
                //     await UpdateShow(seasons);
                //
                //     await _binService.AddItem(season!);
                // }
            }
        }
    }

    private async Task HandleDelete(IEnumerable<string> paths, IEnumerable<InventoryItem>? items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                var addons = await _addonService.ListItems(i => i.InventoryItemId == item.Id);
                
                if (addons != null)
                {
                    var addonPaths = _addonService.GetPaths(item.FolderPath ?? Path.Combine(Globals.MediaFolder, item.Category + "s"), SearchOption.TopDirectoryOnly);
                    
                    foreach (var addon in addons)
                    {
                        if (!addonPaths.Contains(addon.Path))
                        {
                            await _addonService.DeleteAddon(addon.Id);
                        }
                    }
                }

                var versions = await versionService.ListItems(i => i.InventoryItemId == item.Id);

                if (versions != null)
                {
                    foreach (var version in versions)
                    {
                        if (!paths.Contains(version.Path))
                        {
                            await versionService.DeleteVersion(version.Id);

                            await _fileInfo.DeleteFileInfoByParentId(item.Category, version.Id);
                        }
                    }

                    if (!versions.Any())
                    {
                        await UpdateSeason(items);
                        
                        await _binService.AddItem(item);
                    }
                }
            }
        }
    }

    private static IEnumerable<string> GetPaths(string path)
    {
        string[] mediaExtensions =
        [
            ".mp4",
            ".mkv",
            ".avi",
            ".mov",
            ".wmv",
            ".flv",
            ".mp3",
            ".aac",
            ".wav",
            ".flac",
            ".webm",
            ".m4b",
            ".epub",
            ".pdf"
        ];

        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(n => mediaExtensions.Contains(Path.GetExtension(n), StringComparer.InvariantCultureIgnoreCase));
        return files;
    }
}
