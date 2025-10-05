using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Discovery;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services.Discovery;

public class DiscoveryAudiobookService(
    ILogger<DiscoveryAudiobookService> logger,
    IFileInfoService fileInfoService,
    IInventoryService inventoryService,
    IMetadataService metadataService,
    IVersionService versionService,
    IPartService partService,
    IAudioBookMetadataService audioBookMetadataService,
    IBinService binService)
    : IDiscoveryAudiobookService
{
    public async Task CreateAudiobook(string path)
    {
        logger.LogTrace("Creating audiobook for path: {Path}", path);

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

            var books = await inventoryService.ListItems("Audiobook");
            InventoryItem? existingBook = null;
            
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
            
            var existingVersion = (await versionService.List(i => i.Path == folderPath) ?? []).FirstOrDefault();

            if (existingBook != null)
            {
                if (!string.IsNullOrEmpty(folderTitle))
                {
                    if (existingVersion != null)
                    {
                        var existingParts = await partService.ListItems(i => i.InventoryItemVersionId == existingVersion.Id);
                        
                        if (existingParts?.Any(i => i.Path == path) ?? false)
                        {
                            return;
                        }

                        var newPartId = Guid.NewGuid();
                        var part = new InventoryItemPart()
                        {
                            Id = newPartId,
                            Name = partTitle,
                            InventoryItemVersionId = existingVersion.Id,
                            Path = path,
                            FileInfoId = (await fileInfoService.CreateFileInfo(path, newPartId, "Audiobook"))?.Id,
                            PrimaryIdentifier = discNr,
                            SecondaryIdentifier = trackNr
                        };
                        
                        await partService.UpdateOrInsert(part);
                        
                        return;
                    }

                    var version = new InventoryItemVersion
                    {
                        Id = Guid.NewGuid(),
                        Path = folderPath,
                        InventoryItemId = existingBook.Id,
                    };

                    // Do this after the path check because a file info will be created
                    version.FileInfoId = (await fileInfoService.CreateFileInfo(path, version.Id, "Audiobook"))?.Id;
                    
                    await versionService.UpdateOrInsert(version);
                }

                return;
            }

            var versionId = Guid.NewGuid();
            var partId = Guid.NewGuid();
            
            var audiobook = await binService.GetItem<InventoryItem>(title, "Audiobook");

            if (audiobook == null)
            {
                audiobook = new InventoryItem()
                {
                    Id = Guid.NewGuid(),
                    Category = "Audiobook",
                    Title = title,
                };
                
                var metadata = await metadataService.CreateNewMetadata
                (
                    parentId: audiobook.Id,
                    title: audiobook.Title,
                    category: audiobook.Category,
                    path: path
                );

                audiobook.MetadataId = metadata?.Id;
            
                var audiobookMetadata = await audioBookMetadataService.Get(metadata?.AudiobookMetadataId);
            
                audiobook.DisplayImageBlurHash = audiobookMetadata?.ThumbnailBlurHash;
                audiobook.ReleaseDate = DateTime.TryParse(audiobookMetadata?.PublishedDate, out var dateTime)
                    ? dateTime
                    : null;
            }
            
            audiobook.FolderPath = folderPath;

            await inventoryService.AddItem(audiobook);

            var newVersion = new InventoryItemVersion()
            {
                Id = versionId,
                InventoryItemId = audiobook.Id,
                Path = folderPath,
            };
            
            await versionService.UpdateOrInsert(newVersion);

            var newPart = new InventoryItemPart()
            {
                Id = partId,
                InventoryItemVersionId = newVersion.Id,
                Name = partTitle,
                Path = path,
                FileInfoId = (await fileInfoService.CreateFileInfo(path, partId, "Audiobook"))?.Id,
                PrimaryIdentifier = discNr,
                SecondaryIdentifier = trackNr
            };
            
            await partService.UpdateOrInsert(newPart);
            
            await binService.RemoveById(audiobook);
        }
        else
        {
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

            var books = await inventoryService.ListItems("Audiobook");
            
            var existingVersion = (await versionService.List(i => i.Path == path) ?? []).FirstOrDefault();
            var existingBook = await inventoryService.GetItem(existingVersion?.InventoryItemId);

            string? folderPath = null;

            if (!string.IsNullOrEmpty(folderTitle))
            {
                folderPath = path.Replace($"/{title}.{extension}", "");
            }

            if (existingBook != null)
            {
                if (!string.IsNullOrEmpty(folderTitle))
                {
                    var version = new InventoryItemVersion
                    {
                        Id = Guid.NewGuid(),
                        InventoryItemId = existingBook.Id,
                        Path = path,
                    };

                    var versions = await versionService.List(i => i.Path == path);

                    if (versions?.Any(i => i.Path == path) ?? false)
                    {
                        return;
                    }

                    // Do this after the path check because a file info will be created
                    version.FileInfoId = (await fileInfoService.CreateFileInfo(path, version.Id, "Audiobook"))?.Id;

                    await versionService.UpdateOrInsert(version);
                }

                return;
            }

            var versionId = Guid.NewGuid();
            
            var audiobook = await binService.GetItem<InventoryItem>(title, "Audiobook");

            if (audiobook == null)
            {
                audiobook = new InventoryItem()
                {
                    Id = Guid.NewGuid(),
                    Category = "Audiobook",
                    Title = title,
                };

                var metadata = await metadataService.CreateNewMetadata
                (
                    parentId: audiobook.Id,
                    title: audiobook.Title,
                    category: audiobook.Category,
                    path: path
                );

                audiobook.MetadataId = metadata?.Id;
            
                var audiobookMetadata = await audioBookMetadataService.Get(metadata?.AudiobookMetadataId);
            
                audiobook.DisplayImageBlurHash = audiobookMetadata?.ThumbnailBlurHash;
                audiobook.ReleaseDate = DateTime.TryParse(audiobookMetadata?.PublishedDate, out var dateTime)
                    ? dateTime
                    : null;
            }
            
            audiobook.FolderPath = folderPath;

            await inventoryService.AddItem(audiobook);

            var newVersion = new InventoryItemVersion()
            {
                Id = versionId,
                InventoryItemId = audiobook.Id,
                Path = path,
                FileInfoId = (await fileInfoService.CreateFileInfo(path, versionId, "Audiobook"))?.Id
            };
            
            await versionService.UpdateOrInsert(newVersion);
            
            await binService.RemoveById(audiobook);
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