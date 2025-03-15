using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class DiscoveryBookService : IDiscoveryBookService
{
    private readonly ILogger<DiscoveryBookService> _logger;
    private readonly IFileInfoService _fileInfoService;
    private readonly IInventoryService _inventoryService;
    private readonly IMetadataService _metadataService;

    public DiscoveryBookService(ILogger<DiscoveryBookService> logger, IFileInfoService fileInfoService, IInventoryService inventoryService, IMetadataService metadataService)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _inventoryService = inventoryService;
        _metadataService = metadataService;
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

        var books = await _inventoryService.ListItems<Book>("Book");
        var existingBooks = books?.Where(i => i.Versions?.Any(i => i.Path == path) ?? false).FirstOrDefault();

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

                if (existingBooks.Versions?.Any(i => i.Path == path) ?? false)
                {
                    return;
                }

                existingBooks.Versions = existingBooks.Versions?.Append(version);

                await _inventoryService.UpdateByTitle(existingBooks);
            }

            return;
        }

        var versionId = Guid.NewGuid();
        var book = new Book()
        {
            Id = Guid.NewGuid(),
            Versions =
            [
                new()
                {
                    Id = versionId,
                    Path = path,
                }
            ],
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

        await _inventoryService.AddItem(book);
    }
}
