using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Discovery;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services.Discovery;

public class ContentDiscoveryService(
    ILogger<ContentDiscoveryService> logger,
    IDiscoveryShowService showService,
    IDiscoveryMovieService movieService,
    IDiscoveryBookService bookService,
    IInventoryService inventoryService,
    IBinService binService,
    IAddonService addonService,
    IFileInfoService fileInfo,
    IDiscoveryAudiobookService audiobookDiscoveryService,
    IVersionService versionService) : IContentDiscoveryService
{
    private FileSystemWatcher? _watcher;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private int _waitingCount = 0;

    /// <summary>
    /// This method aims to do a full cleanup of the inventory. It first checks if there is something which was deleted and moves those 
    /// entries to the bin folder. After that a new scan can check if an entry like this has already existed before and reuse the same id. 
    /// </summary>
    /// <returns></returns>
    public async Task MoveToBinIfDeleted()
    {
        var paths = GetPaths(Globals.MediaFolder).ToList();

        var movies = await inventoryService.ListItems("Movie");

        await HandleDelete(paths, movies);

        var books = await inventoryService.ListItems("Book");

        await HandleDelete(paths, books);

        var audiobooks = await inventoryService.ListItems("Audiobook");

        await HandleDelete(paths, audiobooks);

        var episodes = await inventoryService.ListItems("Episode");

        await HandleDelete(paths, episodes);
    }

    /// <summary>
    /// Searches for new items but does not delete old ones. If something was deleted by Rescan beforehand and is now in bin it will be restored.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task ActiveScan(string path)
    {
        logger.LogInformation("Scanning {Path}", path);

        if (_waitingCount > 0)
        {
            logger.LogDebug("Skipping active scan because one is already waiting.");

            return;
        }

        Interlocked.Increment(ref _waitingCount);
        await _semaphore.WaitAsync();
        Interlocked.Decrement(ref _waitingCount);

        // This must be done before any scanning so that deleted items have been moved to the bin
        await MoveToBinIfDeleted();

        var files = GetPaths(path);

        try
        {
            await CreateFromPaths(files);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create files from {Path}", path);
            
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task CreateFromPaths(IEnumerable<string> paths)
    {
        logger.LogTrace("Creating from path");

        foreach (var path in paths)
        {
            var category = path.Split('/')[2];

            switch (category)
            {
                case "Movies":
                {
                    await movieService.CreateMovie(path);

                    break;
                }
                case "Shows":
                {
                    await showService.CreateShow(path);

                    break;
                }
                case "Books":
                {
                    await bookService.CreateBook(path);

                    break;
                }
                case "Audiobooks":
                {
                    await audiobookDiscoveryService.CreateAudiobook(path);

                    break;
                }
                case "default":
                {
                    logger.LogWarning("Unknown category: {Category}", category);

                    break;
                }
            }
        }
    }

    public void Watch(string path)
    {
        logger.LogInformation("Watching file system at {Path}", path);

        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            throw new DirectoryNotFoundException($"Watch path not found: {path}");

        // Dispose any previous watcher
        _watcher?.Dispose();

        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            // Filter = "*",
            NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName |
                NotifyFilters.LastWrite |
                NotifyFilters.Size |
                NotifyFilters.CreationTime,
            // Increase buffer to reduce overflow risk (64 KiB; must be a multiple of 4 KB)
            InternalBufferSize = 64 * 1024
        };

        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnChanged;
        _watcher.Renamed += OnChanged;

        _watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        logger.LogInformation("FileSystem changed: {ChangeType} {Path}", e.ChangeType, e.FullPath);

        _ = Task.Run(async () =>
        {
            try
            {
                await ActiveScan(Globals.MediaFolder);
            }
            catch (Exception ex)
            {
                // Prevent unobserved-task exceptions
                logger.LogError(ex, "Active scan failed");
            }
        });
    }

    private async Task UpdateShow(IEnumerable<InventoryItem>? seasons)
    {
        if (seasons != null)
        {
            foreach (var season in seasons)
            {
                var show = await inventoryService.GetItem(season.ShowId);

                var seasonsWithId = await inventoryService.ListItems("Season", i => i.Id == season.ShowId);
                var seasonIds = seasonsWithId?.Select(i => i.Id).ToList();

                if (show != null && (!seasonIds?.Any() ?? true))
                {
                    await binService.AddItem(show!);
                    await inventoryService.Remove(show!);
                }
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
                var season = await inventoryService.GetItem(episode.SeasonId);

                if (season != null)
                {
                    seasons.Add(season);
                }
            }

            foreach (var episode in items)
            {
                var season = seasons.FirstOrDefault(i => i.Id == episode.SeasonId);

                var episodes = await inventoryService.ListItems("Episode", i => i.Id == episode.SeasonId);
                var episodeIDs = episodes?.Select(i => i.Id).ToList();

                if (season != null && (!episodeIDs?.Any() ?? true))
                {
                    await UpdateShow(seasons);

                    await binService.AddItem(season!);
                }
            }
        }
    }

    private async Task HandleDelete(IEnumerable<string> paths, IEnumerable<InventoryItem>? items)
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                var addons = await addonService.ListItems(i => i.InventoryItemId == item.Id);

                if (addons != null)
                {
                    var addonPaths = addonService.GetPaths(
                        item.FolderPath ?? Path.Combine(Globals.MediaFolder, item.Category + "s"),
                        SearchOption.TopDirectoryOnly);

                    foreach (var addon in addons)
                    {
                        if (!addonPaths.Contains(addon.Path))
                        {
                            await addonService.DeleteAddon(addon.Id);
                        }
                    }
                }

                var versions = await versionService.List(i => i.InventoryItemId == item.Id);

                if (versions != null)
                {
                    foreach (var version in versions)
                    {
                        if (!paths.Contains(version.Path))
                        {
                            await versionService.Delete(version.Id);

                            await fileInfo.DeleteFileInfo(version.Id);
                        }
                    }

                    if (!versions.Any())
                    {
                        await UpdateSeason(items);

                        await binService.AddItem(item);
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

        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(n =>
            mediaExtensions.Contains(Path.GetExtension(n), StringComparer.InvariantCultureIgnoreCase));
        return files;
    }
}