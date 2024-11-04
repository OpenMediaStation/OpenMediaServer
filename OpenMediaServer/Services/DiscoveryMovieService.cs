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

    private readonly string _folderRegex = @"^(?<title>[ \w\.\-'`´]+?)(?: ?\((?<language>[A-Z][a-zA-Z]+)\) ?)?(?:[\.\( ](?<year>\d{4})[\.\) ]?)?$";
    private readonly string _fileRegex = @"^(?<title>[ \w\.\-'`´]+?)(?: ?\((?<language>[A-Z][a-zA-Z]+)\) ?)?(?:[\.\( ](?<year>\d{4})[\.\) ]?)?(?: ?\- ?(?<version>[\w ]+))?(?:\.(?<extension>\w{3,4}))$"; 
    //OldRegex: @"(?<category>(Movies|\w+?))/.*?(?<folderTitle>[ \w.-]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<title>[ \w.-.']+?)(?:\((?<year>\d{4})\)|\((?<addition>\D+?)\))?(?:\s*[-.]\s*(?<hyphenAddition>[ \w.-]+?))?([-\.](?<fileInfo>[\w.]*?))?\.(?<extension>\S{3,4})$";
    
    public async Task CreateMovie(string path)
    {
        var fileRegex = new Regex
        (
            pattern: _fileRegex,
            options: RegexOptions.Compiled
        );
        var folderRegex = new Regex
        (
            pattern: _folderRegex,
            options: RegexOptions.Compiled
        );

        var parts = path.Replace(Globals.MediaFolder+Path.DirectorySeparatorChar, string.Empty).Split(Path.DirectorySeparatorChar);

        var lastTwoParts = parts.TakeLast(2).ToArray();
        
        var filePart = lastTwoParts.Last();
        var folderPart = parts.Length >2 ? lastTwoParts.First() : string.Empty;
        
        //todo if(fileName matches "part1" or "cd1" or similar. match on folder instead.(for Title))
        
        var match = fileRegex.Match(filePart);
        if (!match.Success)
        {
            _logger.LogWarning("Invalid path: {Path}", path);
            return;
        }
        
        var folderMatch = folderRegex.Match(folderPart);
        
        var fileGroups = match.Groups;
        var folderGroups = folderMatch.Groups;
        
        var category = "Movie";
        var folderTitle = folderGroups["title"].Value;
        var title = fileGroups["title"].Value.Replace("."," ").Trim();
        var versionName = fileGroups["version"].Value;
        
        _logger.LogDebug("Movie detected");

        var movies = await _inventoryService.ListItems<Movie>("Movie");
        var existingMovie = movies?.Where(i => i.Versions?.Any(j => j.Path == path) ?? false).FirstOrDefault();

        string? folderPath = null;

        if (!string.IsNullOrEmpty(folderTitle))
        {
            folderPath = Path.Combine(parts.Take(parts.Length - 1).Prepend(Globals.MediaFolder).ToArray());
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
                    Name = versionName
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
            Title = //!string.IsNullOrEmpty(hypenAddition) && folderPath != null ? $"{title} - {hypenAddition}" :
                title,
            FolderPath = folderPath
        };

        var metadata = await _metadataService.CreateNewMetadata
        (
            parentId: movie.Id,
            title: movie.Title,
            category: movie.Category,
            year: fileGroups.TryGetValue("year", out var movieTitleYear) ?
                    movieTitleYear.Value : fileGroups.TryGetValue("folderYear", out var movieFolderYear) ?
                    movieFolderYear.Value : null
        );

        movie.MetadataId = metadata?.Id;

        await _inventoryService.AddItem(movie);
    }
}