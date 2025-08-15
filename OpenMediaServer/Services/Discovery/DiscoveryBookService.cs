using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services.Discovery;

public class DiscoveryBookService(
    ILogger<DiscoveryBookService> logger,
    IFileInfoService fileInfoService,
    IInventoryService inventoryService,
    IMetadataService metadataService,
    IVersionService versionService,
    IBookMetadataService bookMetadataService)
    : IDiscoveryBookService
{
    public async Task CreateBook(string path)
    {
        logger.LogTrace("Creating book for path: {Path}", path);

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
            logger.LogWarning("SplittedTitle null.... Invalid path: {Path}", path);
            return;
        }

        var title = string.Join(".", splittedTitle);

        if (extension == null || title == null)
        {
            logger.LogWarning("Invalid path: {Path}", path);
            return;
        }

        var books = await inventoryService.ListItems("Book");

        var existingVersion = (await versionService.ListItems(i => i.Path == path) ?? []).FirstOrDefault();
        var existingBooks = await inventoryService.GetItem(existingVersion?.InventoryItemId);

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

                var versions = await versionService.ListItems(i => i.Path == path);

                if (versions?.Any(i => i.Path == path) ?? false)
                {
                    return;
                }

                await versionService.UpdateOrInsert(version);
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

        var metadata = await metadataService.CreateNewMetadata
        (
            parentId: book.Id,
            title: book.Title,
            category: book.Category
        );
        
        var bookMetadata = await bookMetadataService.Get(metadata?.BookMetadataId);

        book.MetadataId = metadata?.Id;
        book.DisplayImageBlurHash = bookMetadata?.ThumbnailBlurHash;
        book.ReleaseDate = DateTime.TryParse(bookMetadata?.PublishedDate, out var dateTime) ? dateTime : null;

        await inventoryService.AddItem(book);

        var newVersion = new InventoryItemVersion()
        {
            Id = versionId,
            Path = path,
            InventoryItemId = book.Id
        };

        await versionService.UpdateOrInsert(newVersion);
    }
}