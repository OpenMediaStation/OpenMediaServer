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
    private readonly IVersionService _versionService;

    public DiscoveryAudiobookService(ILogger<DiscoveryAudiobookService> logger, IFileInfoService fileInfoService,
        IInventoryService inventoryService, IMetadataService metadataService, IVersionService versionService)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _inventoryService = inventoryService;
        _metadataService = metadataService;
        _versionService = versionService;
    }

    public async Task CreateAudiobook(string path)
    {
        _logger.LogTrace("Creating audiobook for path: {Path}", path);

        var splittedPath = path.Split("/");

        var folderTitle = (splittedPath.Length - 2) >= 0 ? splittedPath[^2] : null;

        if (folderTitle == "Audiobooks")
        {
            folderTitle = null;
        }

        var extension = splittedPath.LastOrDefault()?.Split(".").LastOrDefault();

        if (IsPart(splittedPath))
        {
            var title = splittedPath[^2];
            string partTitle = splittedPath[^1].Replace($".{extension}", "");
            bool disc = false;
            int? discNr = null;
            int? trackNr = null;

            var lastPartTotal = partTitle.Split(" ").LastOrDefault();
            if (int.TryParse(lastPartTotal, out int resultLastPartTotal))
            {
                trackNr = resultLastPartTotal;
            }

            if (title.ToLower().StartsWith("disc"))
            {
                var lastPart = title.Split(" ").LastOrDefault();
                if (int.TryParse(lastPart, out int result))
                {
                    discNr = result;
                }

                title = splittedPath[^3];
                disc = true;
                folderTitle = (splittedPath.Length - 3) >= 0 ? splittedPath[^3] : null;
            }

            var books = await _inventoryService.ListItems("Audiobook");
            var existingVersion = (await _versionService.ListItems(i => i.Path == path)).FirstOrDefault();
            var existingVersionId = existingVersion?.Id;

            InventoryItem? existingBook = null;

            if (existingVersionId != null)
            {
                existingBook = await _inventoryService.GetItem((Guid)existingVersionId);
            }


            string? folderPath = null;

            if (!string.IsNullOrEmpty(folderTitle))
            {
                folderPath = path.Replace(splittedPath.LastOrDefault() ?? "", "");

                if (disc)
                {
                    folderPath = folderPath.Replace($"{splittedPath[^2]}/", "");
                }

                existingBook = books?.Where(i => i.FolderPath == folderPath).FirstOrDefault();
            }

            if (existingBook != null)
            {
                if (!string.IsNullOrEmpty(folderTitle))
                {
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
                            Name = partTitle,
                            Path = path,
                            FileInfoId = (await _fileInfoService.CreateFileInfo(path, newPartId, "Audiobook"))?.Id,
                            PrimaryIdentifier = discNr,
                            SecondaryIdentifier = trackNr
                        });
                        
                        await _versionService.UpdateOrInsert(existingVersion);
                        
                        return;
                    }

                    var version = new InventoryItemVersion
                    {
                        Id = Guid.NewGuid(),
                        Path = path,
                    };

                    // Do this after the path check because a file info will be created
                    version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, "Audiobook"))?.Id;
                    
                    await _versionService.UpdateOrInsert(version);
                }

                return;
            }

            var versionId = Guid.NewGuid();
            var partId = Guid.NewGuid();

            var audiobook = new InventoryItem()
            {
                Id = Guid.NewGuid(),
                Category = "Audiobook",
                Title = title,
                FolderPath = folderPath
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: audiobook.Id,
                title: audiobook.Title,
                category: audiobook.Category,
                path: path
            );

            audiobook.MetadataId = metadata?.Id;
            audiobook.DisplayImageBlurHash = metadata?.Audiobook?.ThumbnailBlurHash;
            audiobook.ReleaseDate = DateOnly.TryParse(metadata?.Audiobook?.PublishedDate, out var dateOnly)
                ? dateOnly
                : null;

            await _inventoryService.AddItem(audiobook);

            var newVersion = new InventoryItemVersion()
            {
                Id = versionId,
                InventoryItemId = audiobook.Id,
                Path = folderPath,
                Parts =
                [
                    new()
                    {
                        Id = partId,
                        Name = partTitle,
                        Path = path,
                        FileInfoId = (await _fileInfoService.CreateFileInfo(path, partId, "Audiobook"))?.Id,
                        PrimaryIdentifier = discNr,
                        SecondaryIdentifier = trackNr
                    }
                ]
            };
            
            await _versionService.UpdateOrInsert(newVersion);
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

            var books = await _inventoryService.ListItems("Audiobook");
            
            var existingVersion = (await _versionService.ListItems(i => i.Path == path)).FirstOrDefault();
            var existingBook = await _inventoryService.GetItem((Guid)existingVersion.InventoryItemId);

            string? folderPath = null;

            if (!string.IsNullOrEmpty(folderTitle))
            {
                folderPath = path.Replace($"/{title}.{extension}", "");
                existingBook = books?.Where(i => i.FolderPath == folderPath).FirstOrDefault();
            }

            if (existingBook != null)
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

                    // Do this after the path check because a file info will be created
                    version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, "Audiobook"))?.Id;

                    await _versionService.UpdateOrInsert(version);
                }

                return;
            }

            var versionId = Guid.NewGuid();
            var audiobook = new InventoryItem()
            {
                Id = Guid.NewGuid(),
                Category = "Audiobook",
                Title = title,
                FolderPath = folderPath
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: audiobook.Id,
                title: audiobook.Title,
                category: audiobook.Category,
                path: path
            );

            audiobook.MetadataId = metadata?.Id;
            audiobook.DisplayImageBlurHash = metadata?.Audiobook?.ThumbnailBlurHash;
            audiobook.ReleaseDate = DateOnly.TryParse(metadata?.Audiobook?.PublishedDate, out var dateOnly)
                ? dateOnly
                : null;

            await _inventoryService.AddItem(audiobook);

            var newVersion = new InventoryItemVersion()
            {
                Id = versionId,
                InventoryItemId = audiobook.Id,
                Path = path,
                FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Audiobook"))?.Id
            };
            
            await _versionService.UpdateOrInsert(newVersion);
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