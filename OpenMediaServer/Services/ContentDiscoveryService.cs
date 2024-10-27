using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Services;

public class ContentDiscoveryService(ILogger<ContentDiscoveryService> logger, IDiscoveryShowService showService, IDiscoveryMovieService movieService, IDiscoveryBookService _bookService) : IContentDiscoveryService
{
    private readonly ILogger<ContentDiscoveryService> _logger = logger;
    private readonly IDiscoveryShowService _showService = showService;
    private readonly IDiscoveryMovieService _movieService = movieService;
    private readonly IDiscoveryBookService _bookService = _bookService;

    public async Task ActiveScan(string path)
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
        
        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(n=> mediaExtensions.Contains(Path.GetExtension(n),StringComparer.InvariantCultureIgnoreCase));
         
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
}
