using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

// TODO Maybe split this service in two => movie and shows separate

public class DiscoveryMovieShowService(ILogger<DiscoveryMovieShowService> logger, IFileInfoService fileInfoService, IMetadataService metadataService, IInventoryService inventoryService) : IDiscoveryMovieShowService
{
    private readonly ILogger<DiscoveryMovieShowService> _logger = logger;
    private readonly IFileInfoService _fileInfoService = fileInfoService;
    private readonly IMetadataService _metadataService = metadataService;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly string _regex = @"(?<category>(Movies)|(Shows)|\w+?)/.*?(?<folderTitle>[ \w.]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<seasonFolder>(([sS]taffel ?)|([Ss]eason ?))\d+)?/?(?<title>([ \w\.]+?))((\(|\.)(?<year>\d{4})(\)|\.?))?(([sS](?<season>\d*))([eE](?<episode>\d+)))?((-|\.)(?<fileInfo>[\w\.]*?))?\.(?<extension>\S{3,4})$";

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
        var title = groups["title"].Value;

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
            Title = title,
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

    public async Task CreateShow(string path)
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
        var title = groups["title"].Value;


        _logger.LogDebug("Show detected");

        // Show
        var showPath = Path.Combine(Globals.MediaFolder, "Shows", folderTitle);
        var show = await _inventoryService.GetItem<Show>("Show", i => i.FolderPath == showPath);

        if (show == null)
        {
            show = new Show
            {
                Id = Guid.NewGuid(),
                Title = folderTitle,
            };

            show.FolderPath = Path.Combine(Globals.MediaFolder, "Shows", folderTitle);

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: show.Id,
                title: show.Title,
                year: groups["yearFolder"].Value,
                category: show.Category
            );

            show.MetadataId = metadata?.Id;

            await _inventoryService.AddItem(show);
        }

        // Season
        var season = await _inventoryService.GetItem<Season>("Season", i => i.FolderPath == Directory.GetParent(path)?.FullName);

        if (season == null)
        {
            season = new Season
            {
                Id = Guid.NewGuid(),

                ShowId = show.Id,
                Title = groups["seasonFolder"].Value,
                SeasonNr = int.TryParse(groups["season"].Value, out var seasonNr) ? seasonNr : null,
                FolderPath = Directory.GetParent(path)?.FullName ?? Directory.GetCurrentDirectory()
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: season.Id,
                title: show.Title,
                year: groups["yearFolder"].Value,
                category: season.Category,
                season: seasonNr
            );

            season.MetadataId = metadata?.Id;

            await _inventoryService.AddItem(season);

            show.SeasonIds ??= [];
            show.SeasonIds = show.SeasonIds.Append(season.Id);

            await _inventoryService.UpdateById(show);
        }

        // Episode
        var episode = await _inventoryService.GetItem<Episode>("Episode", predicate: i => i.Versions?.Any(i => i.Path == path) ?? false);

        if (episode == null)
        {
            var versionId = Guid.NewGuid();
            episode = new Episode
            {
                Id = Guid.NewGuid(),

                SeasonId = season.Id,
                Title = title,
                Versions =
                [
                    new()
                    {
                        Id = versionId,
                        Path = path,
                        FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Episode"))?.Id
                    }
                ],
                EpisodeNr = int.TryParse(groups["episode"].Value, out var episodeNr) ? episodeNr : null,
                SeasonNr = season.SeasonNr
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: episode.Id,
                title: episode.Title,
                year: groups["yearFolder"].Value,
                category: episode.Category,
                episode: episode.EpisodeNr,
                season: episode.SeasonNr
            );

            episode.MetadataId = metadata?.Id;

            await _inventoryService.AddItem(episode);

            season.EpisodeIds ??= [];
            season.EpisodeIds = season.EpisodeIds.Append(episode.Id);

            await _inventoryService.UpdateById(season);
        }
    }
}