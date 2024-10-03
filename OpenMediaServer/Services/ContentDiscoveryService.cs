using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Services;

public class ContentDiscoveryService : IContentDiscoveryService
{
    private readonly ILogger<ContentDiscoveryService> _logger;
    private readonly IInventoryService _inventoryService;

    public ContentDiscoveryService(ILogger<ContentDiscoveryService> logger, IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public async Task ActiveScan(string path)
    {
        string[] mediaExtensions = [".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".mp3", ".aac", ".wav", ".flac", ".webm"];
        var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(n=> mediaExtensions.Contains(Path.GetExtension(n),StringComparer.InvariantCultureIgnoreCase));
         
        await _inventoryService.CreateFromPaths(files);
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
