using System.Linq.Expressions;
using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class AddonService(
    ILogger<AddonService> logger,
    IFileSystemRepository fileSystemRepository,
    IDataRepository dataRepository)
    : IAddonService
{
    public async Task UpdateOrInsert(InventoryItemAddon item)
    {
        await dataRepository.WriteObject(item);
    }

    public async Task<IEnumerable<InventoryItemAddon>?> ListItems(
        Expression<Func<InventoryItemAddon, bool>>? filter = null)
    {
        var items = await dataRepository.ListObjects(filter);

        return items;
    }

    public async Task DeleteAddon(Guid addonId)
    {
        await dataRepository.DeleteObjectWithFilter<InventoryItemAddon>(addonId);
    }

    public async Task<Stream?> DownloadAddon(Guid inventoryItemId, string category, Guid addonId)
    {
        var addons = await ListItems(i => i.InventoryItemId == inventoryItemId && i.Id == addonId);

        var addon = addons?.FirstOrDefault();

        if (addon == null)
        {
            return null;
        }

        var stream = fileSystemRepository.GetStream(addon.Path);

        return stream;
    }

    public IEnumerable<InventoryItemAddon> DiscoverAddons(string path)
    {
        logger.LogDebug("Path: {Path}", path);

        var splitPath = path.Split('/');
        var fileName = splitPath.LastOrDefault();
        var extension = fileName?.Split(".").LastOrDefault();

        if (fileName == null || extension == null)
        {
            return [];
        }

        var fileLocation = new string(path.Reverse().ToArray());
        fileLocation = fileLocation.Substring(fileName.Length);
        fileLocation = new string(fileLocation.Reverse().ToArray());

        var fileNameWithoutExtension = fileName.Replace($".{extension}", "");

        var files = EnumerateFiles(fileLocation, fileNameWithoutExtension);

        var addons = GenerateAddons(files);

        return addons;
    }

    private IEnumerable<InventoryItemAddon> GenerateAddons(IEnumerable<string> files)
    {
        var addons = new List<InventoryItemAddon>();

        foreach (var item in files)
        {
            var extension = item.Split(".").LastOrDefault();
            string category = (extension?.ToLower()) switch
            {
                "vtt" => "Subtitle",
                _ => "Unknown",
            };

            string? sub = null;

            if (category == "Subtitle")
            {
                var dotSplitted = item.Split(".");

                string? lang = null;
                if (dotSplitted.Length >= 3)
                {
                    lang = dotSplitted.SkipLast(1).LastOrDefault();
                }

                sub = lang;
            }

            var addon = new InventoryItemAddon()
            {
                Id = Guid.NewGuid(),
                Path = item,
                Category = category,
                SubtitleLanguage = sub,
            };

            addons.Add(addon);
        }

        return addons;
    }

    public IEnumerable<string> GetPaths(string path, SearchOption searchOption = SearchOption.AllDirectories)
    {
        string[] mediaExtensions =
        [
            ".vtt"
        ];

        try
        {
            var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(n => mediaExtensions.Contains(Path.GetExtension(n), StringComparer.InvariantCultureIgnoreCase));

            return files;
        }
        catch (DirectoryNotFoundException)
        {
            return [];
        }
    }

    private IEnumerable<string> EnumerateFiles(string path, string prefix)
    {
        var files = GetPaths(path);

        var filesWithPrefix = files.Where(i => i.Split("/").LastOrDefault()?.Contains(prefix) ?? false);

        return filesWithPrefix;
    }
}