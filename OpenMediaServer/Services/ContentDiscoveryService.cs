using System;
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

    public void ActiveScan(string path)
    {
        List<string> dirs = new List<string>(Directory.EnumerateDirectories(path));
        _inventoryService.CreateFromPaths(Directory.EnumerateFiles(path));
        foreach (string dir in dirs)
        {
            ActiveScan(dir);
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

        ActiveScan(Globals.DataFolder);
    }
}