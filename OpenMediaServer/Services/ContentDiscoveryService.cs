using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class ContentDiscoveryService(ILogger<ContentDiscoveryService> logger, IDiscoveryShowService showService, IDiscoveryMovieService movieService, IDiscoveryBookService _bookService, IInventoryService _inventoryService, IBinService _binService, IAddonService _addonService, IFileInfoService _fileInfo) : IContentDiscoveryService
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

        var movies = await _inventoryService.ListItems<Movie>("Movie");

        await HandleDelete(paths, movies);

        var books = await _inventoryService.ListItems<Book>("Book");

        await HandleDelete(paths, books);

        var episodes = await _inventoryService.ListItems<Episode>("Episode");

        await HandleDelete(paths, episodes);

        // TODO Shows
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

    private async Task UpdateShow(IEnumerable<Season>? seasons)
    {
        if (seasons != null)
        {
            foreach (var season in seasons)
            {
                var show = await _inventoryService.GetItem<Show>(season.ShowId, "Show");

                if (show != null)
                {
                    var seasonIds = show.SeasonIds?.ToList();
                    seasonIds?.RemoveAll(i => i == season.Id);

                    show.SeasonIds = seasonIds;

                    await _inventoryService.UpdateById(show);
                }

                if (show != null && (!show?.SeasonIds?.Any() ?? true))
                {
                    await _binService.AddItem(show!);
                    await _inventoryService.RemoveById(show!);
                }
            }
        }
    }

    private async Task UpdateSeason(IEnumerable<Episode>? items)
    {
        if (items != null)
        {
            List<Season> seasons = [];

            foreach (var episode in items)
            {
                var season = await _inventoryService.GetItem<Season>(episode.SeasonId, "Season");

                if (season != null)
                {
                    seasons.Add(season);
                }
            }

            foreach (var episode in items)
            {
                var season = seasons.FirstOrDefault(i => i.Id == episode.SeasonId);

                if (season != null)
                {
                    var episodeIds = season.EpisodeIds?.ToList();
                    episodeIds?.RemoveAll(i => i == episode.Id);

                    season.EpisodeIds = episodeIds;

                    await _inventoryService.UpdateById(season);
                }

                if (season != null && (!season?.EpisodeIds?.Any() ?? true))
                {
                    await UpdateShow(seasons);

                    await _binService.AddItem(season!);
                    await _inventoryService.RemoveById(season!);
                }
            }
        }
    }

    private async Task HandleDelete<T>(IEnumerable<string> paths, IEnumerable<T>? items) where T : InventoryItem
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item.Addons != null)
                {
                    var addonPaths = _addonService.GetPaths(item.FolderPath ?? Path.Combine(Globals.MediaFolder, item.Category + "s"), SearchOption.TopDirectoryOnly);

                    foreach (var addon in item.Addons)
                    {
                        if (!addonPaths.Contains(addon.Path))
                        {
                            var temp = item.Addons.ToList();
                            temp.Remove(addon);
                            item.Addons = temp;
                            await _inventoryService.UpdateById(item);
                        }
                    }
                }

                if (item.Versions != null)
                {
                    foreach (var version in item.Versions)
                    {
                        if (!paths.Contains(version.Path))
                        {
                            var temp = item.Versions.ToList();
                            temp.Remove(version);
                            item.Versions = temp;
                            await _inventoryService.UpdateById(item);

                            await _fileInfo.DeleteFileInfoByParentId(item.Category, version.Id);
                        }
                    }

                    if (!item.Versions.Any())
                    {
                        if (typeof(Episode).IsAssignableFrom(typeof(T)))
                        {
                            await UpdateSeason(items as IEnumerable<Episode>);
                        }

                        await _binService.AddItem(item);
                        await _inventoryService.RemoveById(item);
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
