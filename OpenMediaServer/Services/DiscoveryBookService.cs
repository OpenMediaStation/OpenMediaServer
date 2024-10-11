using System;
using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class DiscoveryBookService : IDiscoveryBookService
{
    private readonly ILogger<DiscoveryBookService> _logger;
    private readonly IFileInfoService _fileInfoService;
    private readonly IInventoryService _inventoryService;

    private readonly string _regex = @"(?<category>(Books)|\w+?)/.*?(?<folderTitle>[ \w.-]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?/?(?<title>([ \w\.-]+?))((\(|\.)(?<year>\d{4})(\)|\.?))?((-|\.)(?<fileInfo>[\w\.-]*?))?\.(?<extension>\S{3,4})$";

    public DiscoveryBookService(ILogger<DiscoveryBookService> logger, IFileInfoService fileInfoService, IInventoryService inventoryService)
    {
        _logger = logger;
        _fileInfoService = fileInfoService;
        _inventoryService = inventoryService;
    }

    public async Task CreateBook(string path)
    {
        _logger.LogTrace("Creating book for path: {Path}", path);

        var pathRegex = new Regex
        (
            pattern: _regex,
            options: RegexOptions.Compiled
        );

        var match = pathRegex.Match(path.Replace(Globals.MediaFolder, string.Empty));
        if (!match.Success)
        {
            _logger.LogWarning("Invalid path: {Path}", path);
            return;
        }
        var groups = match.Groups;
        var folderTitle = groups["folderTitle"].Value;
        var title = groups["title"].Value; 
        var extension = groups["extension"].Value; 

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

                // Do this after the path check because a file info will be created
                version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, "Book"))?.Id;

                existingBooks.Versions = existingBooks.Versions?.Append(version);

                await _inventoryService.Update(existingBooks);
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
                    FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Book"))?.Id
                }
            ],
            Title = title,
            FolderPath = folderPath
        };

        // var metadata = await _metadataService.CreateNewMetadata
        // (
        //     parentId: book.Id,
        //     title: book.Title,
        //     category: book.Category,
        //     year: groups.TryGetValue("year", out var movieTitleYear) ?
        //             movieTitleYear.Value : groups.TryGetValue("folderYear", out var movieFolderYear) ?
        //             movieFolderYear.Value : null
        // );

        // book.MetadataId = metadata?.Id;

        await _inventoryService.AddItem(book);
    }
}
