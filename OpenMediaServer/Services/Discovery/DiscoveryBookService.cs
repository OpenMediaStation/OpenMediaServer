using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services.Discovery;

public class DiscoveryBookService : IDiscoveryBookService
{
    private readonly ILogger<DiscoveryBookService> _logger;
    private readonly IFileInfoService _fileInfoService;
    private readonly IInventoryService _inventoryService;
    private readonly IMetadataService _metadataService;
    private readonly IVersionService _versionService;

    public DiscoveryBookService(ILogger<DiscoveryBookService> logger, IFileInfoService fileInfoService,
        IInventoryService inventoryService, IMetadataService metadataService, IVersionService versionService)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _inventoryService = inventoryService;
        _metadataService = metadataService;
        _versionService = versionService;
    }

    public async Task CreateBook(string path)
    {
        _logger.LogTrace("Creating book for path: {Path}", path);

        var splittedPath = path.Split("/");

        var folderTitle = (splittedPath.Length - 2) >= 0 ? splittedPath[^2] : null;

        if (folderTitle == "Books")
        {
            folderTitle = null;
        }

        var extension = splittedPath.LastOrDefault()?.Split(".").LastOrDefault();

        var splittedTitle = splittedPath.LastOrDefault()?.Split(".").SkipLast(1);

        if (splittedTitle == null)
        {
            _logger.LogWarning("SplittedTitle null.... Invalid path: {Path}", path);
            return;
        }

        var title = string.Join(".", splittedTitle);

        if (extension == null || title == null)
        {
            _logger.LogWarning("Invalid path: {Path}", path);
            return;
        }

        var books = await _inventoryService.ListItems("Book");

        var existingVersion = (await _versionService.ListItems(i => i.Path == path)).FirstOrDefault();
        var existingBooks = await _inventoryService.GetItem((Guid)existingVersion.InventoryItemId);

        string? folderPath = null;

        if (!string.IsNullOrEmpty(folderTitle))
        {
            folderPath = path.Replace($"/{title}.{extension}", "");
            existingBooks = books?.Where(i => i.FolderPath == folderPath).FirstOrDefault();
        }

        if (existingBooks != null)
        {
            if (!string.IsNullOrEmpty(folderTitle))
            {
                var version = new InventoryItemVersion
                {
                    Id = Guid.NewGuid(),
                    Path = path,
                };

                var versions = await _versionService.ListItems(i => i.Path == path);

                if (versions?.Any(i => i.Path == path) ?? false)
                {
                    return;
                }

                await _versionService.UpdateOrInsert(version);
            }

            return;
        }

        var versionId = Guid.NewGuid();
        var book = new InventoryItem()
        {
            Id = Guid.NewGuid(),
            Category = "Book",
            Title = title,
            FolderPath = folderPath
        };

        var metadata = await _metadataService.CreateNewMetadata
        (
            parentId: book.Id,
            title: book.Title,
            category: book.Category
        );

        book.MetadataId = metadata?.Id;
        book.DisplayImageBlurHash = metadata?.Book?.ThumbnailBlurHash;
        book.ReleaseDate = DateOnly.TryParse(metadata?.Book?.PublishedDate, out var dateOnly) ? dateOnly : null;

        await _inventoryService.AddItem(book);

        var newVersion = new InventoryItemVersion()
        {
            Id = versionId,
            Path = path,
            InventoryItemId = book.Id
        };

        await _versionService.UpdateOrInsert(newVersion);
    }
}