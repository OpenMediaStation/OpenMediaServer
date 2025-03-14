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

        var splittedPath = path.Split("/");

        var folderTitle = (splittedPath.Length - 2) >= 0 ? splittedPath[^2] : null;
        var extension = splittedPath.LastOrDefault()?.Split(".").LastOrDefault();

        if (IsPart(splittedPath))
        {
            var title = splittedPath[^2];

            var books = await _inventoryService.ListItems<Audiobook>("Audiobook");
            var existingBook = books?.Where(i => i.Versions?.Any(i => i.Path == path) ?? false).FirstOrDefault();

            string? folderPath = null;

            if (!string.IsNullOrEmpty(folderTitle))
            {
                folderPath = path.Replace(splittedPath.LastOrDefault() ?? "", "");
                existingBook = books?.Where(i => i.FolderPath == folderPath).FirstOrDefault();
            }

            if (existingBook != null)
            {
                if (!string.IsNullOrEmpty(folderTitle))
                {
                    var existingVersion = existingBook.Versions?.Where(i => i.Path == folderPath).FirstOrDefault();
                    if (existingVersion != null)
                    {
                        if (existingVersion.Parts?.Any(i => i.Path == path) ?? false)
                        {
                            return;
                        }

                        var newPartId = Guid.NewGuid();
                        existingVersion.Parts = existingVersion.Parts?.Append(new()
                        {
                            Id = newPartId,
                            Name = splittedPath[^1].Replace($".{extension}", ""),
                            Path = path,
                            FileInfoId = (await _fileInfoService.CreateFileInfo(path, newPartId, "Audiobook"))?.Id
                        });

                        var temp = existingBook.Versions?.ToList();
                        temp?.RemoveAll(i => i.Id == existingVersion.Id);
                        temp?.Add(existingVersion);
                        existingBook.Versions = temp;

                        await _inventoryService.UpdateByTitle(existingBook);

                        return;
                    }

                    var version = new InventoryItemVersion
                    {
                        Id = Guid.NewGuid(),
                        Path = path,
                    };

                    // Do this after the path check because a file info will be created
                    version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, "Audiobook"))?.Id;

                    existingBook.Versions = existingBook.Versions?.Append(version);

                    await _inventoryService.UpdateByTitle(existingBook);
                }

                return;
            }

            var versionId = Guid.NewGuid();
            var partId = Guid.NewGuid();

            var audiobook = new Audiobook()
            {
                Id = Guid.NewGuid(),
                Versions =
                [
                    new()
                    {
                        Id = versionId,
                        Path = folderPath,
                        Parts =
                        [
                            new()
                            {
                                Id = partId,
                                Name = splittedPath[^1].Replace($".{extension}", ""),
                                Path = path,
                                FileInfoId = (await _fileInfoService.CreateFileInfo(path, partId, "Audiobook"))?.Id
                            }
                        ]
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
        else
        {
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

    private bool IsPart(IEnumerable<string> path)
    {
        if (path.LastOrDefault()?.ToLower().StartsWith("track") ?? false)
        {
            return true;
        }

        return false;
    }
}
