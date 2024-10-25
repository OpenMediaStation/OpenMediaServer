using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class DiscoveryMovieService(ILogger<DiscoveryMovieService> logger, IFileInfoService fileInfoService, IMetadataService metadataService, IInventoryService inventoryService) : IDiscoveryMovieService
{
    private readonly ILogger<DiscoveryMovieService> _logger = logger;
    private readonly IFileInfoService _fileInfoService = fileInfoService;
    private readonly IMetadataService _metadataService = metadataService;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly string _regex = @"(?<category>(Movies|\w+?))/.*?(?<folderTitle>[ \w.-]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<title>[ \w.-.']+?)(?:\((?<year>\d{4})\)|\((?<addition>\D+?)\))?(?:\s*[-.]\s*(?<hyphenAddition>[ \w.-]+?))?([-\.](?<fileInfo>[\w.]*?))?\.(?<extension>\S{3,4})$";

    public async Task CreateMovie(string path)
    {
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
        var category = groups["category"].Value;
        var folderTitle = groups["folderTitle"].Value;
        var title = groups["title"].Value.Trim();
        var addition = groups["addition"].Value;
        var hypenAddition = groups["hyphenAddition"].Value;

        _logger.LogDebug("Movie detected");

        var movies = await _inventoryService.ListItems<Movie>("Movie");
        var existingMovie = movies?.Where(i => i.Versions?.Any(i => i.Path == path) ?? false).FirstOrDefault();

        string? folderPath = null;

        if (!string.IsNullOrEmpty(folderTitle))
        {
            folderPath = Path.Combine(Globals.MediaFolder, category, folderTitle);
            existingMovie = movies?.Where(i => i.FolderPath == folderPath).FirstOrDefault();
        }

        if (existingMovie != null)
        {
            if (!string.IsNullOrEmpty(folderTitle))
            {
                var version = new InventoryItemVersion
                {
                    Id = Guid.NewGuid(),
                    Path = path,
                };

                if (existingMovie.Versions?.Any(i => i.Path == path) ?? false)
                {
                    return;
                }

                // Do this after the path check because a file info will be created
                version.FileInfoId = (await _fileInfoService.CreateFileInfo(path, version.Id, category))?.Id;

                existingMovie.Versions = existingMovie.Versions?.Append(version);

                await _inventoryService.UpdateByTitle(existingMovie);
            }

            return;
        }

        var versionId = Guid.NewGuid();
        var movie = new Movie()
        {
            Id = Guid.NewGuid(),
            Versions =
            [
                new()
                {
                    Id = versionId,
                    Path = path,
                    FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, category))?.Id
                }
            ],
            Title = !string.IsNullOrEmpty(hypenAddition) && folderPath != null ? $"{title} - {hypenAddition}" : title,
            FolderPath = folderPath
        };

        var metadata = await _metadataService.CreateNewMetadata
        (
            parentId: movie.Id,
            title: movie.Title,
            category: movie.Category,
            year: groups.TryGetValue("year", out var movieTitleYear) ?
                    movieTitleYear.Value : groups.TryGetValue("folderYear", out var movieFolderYear) ?
                    movieFolderYear.Value : null
        );

        movie.MetadataId = metadata?.Id;

        await _inventoryService.AddItem(movie);
    }
}