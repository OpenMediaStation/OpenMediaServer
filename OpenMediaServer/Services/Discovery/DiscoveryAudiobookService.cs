using System;
using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Discovery;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services.Discovery;

public class DiscoveryAudiobookService : IDiscoveryAudiobookService
{
    private readonly ILogger<DiscoveryAudiobookService> _logger;
    private readonly IFileInfoService _fileInfoService;
    private readonly IInventoryService _inventoryService;
    private readonly IMetadataService _metadataService;

    public DiscoveryAudiobookService(ILogger<DiscoveryAudiobookService> logger, IFileInfoService fileInfoService, IInventoryService inventoryService, IMetadataService metadataService)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _inventoryService = inventoryService;
        _metadataService = metadataService;
    }

    public async Task CreateAudiobook(string path)
    {
        _logger.LogTrace("Creating audiobook for path: {Path}", path);

        if (IsPart(path))
        {

        }
        else
        {
            var splittedPath = path.Split("/");

            var folderTitle = (splittedPath.Length - 2) >= 0 ? splittedPath[^2] : null;
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

            var books = await _inventoryService.ListItems<Audiobook>("Audiobook");
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

                    // Do this after the path check because a file info will be created
                    version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, "Audiobook"))?.Id;

                    existingBooks.Versions = existingBooks.Versions?.Append(version);

                    await _inventoryService.UpdateByTitle(existingBooks);
                }

                return;
            }

            var versionId = Guid.NewGuid();
            var audiobook = new Audiobook()
            {
                Id = Guid.NewGuid(),
                Versions =
                [
                    new()
                {
                    Id = versionId,
                    Path = path,
                    FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Audiobook"))?.Id
                }
                ],
                Title = title,
                FolderPath = folderPath
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: audiobook.Id,
                title: audiobook.Title,
                category: audiobook.Category
            );

            audiobook.MetadataId = metadata?.Id;

            await _inventoryService.AddItem(audiobook);
        }
    }

    private bool IsPart(string path)
    {
        // TODO
        return false;
    }
}
