using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Services;

public class ContentDiscoveryService : IContentDiscoveryService
{
    private readonly ILogger<ContentDiscoveryService> _logger;
    private readonly IDiscoveryMovieShowService _movieShowService;

    public ContentDiscoveryService(ILogger<ContentDiscoveryService> logger, IDiscoveryMovieShowService movieShowService)
    {
        _logger = logger;
        _movieShowService = movieShowService;
    }

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
                        await _movieShowService.CreateMovie(path);

                        break;
                    }
                case "Shows":
                    {
                        await _movieShowService.CreateShow(path);

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
        watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("FileSystem changed");

        ActiveScan(Globals.MediaFolder).Wait(); // TODO Might be problematic
    }
}
