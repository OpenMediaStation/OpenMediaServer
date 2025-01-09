using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class DiscoveryShowService(ILogger<DiscoveryShowService> logger, IFileInfoService fileInfoService, IMetadataService metadataService, IInventoryService inventoryService) : IDiscoveryShowService
{
    private readonly ILogger<DiscoveryShowService> _logger = logger;
    private readonly IFileInfoService _fileInfoService = fileInfoService;
    private readonly IMetadataService _metadataService = metadataService;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly string _regex = @"(?<category>(Shows)|\w+?)/.*?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<seasonFolder>(([sS]taffel ?)|([Ss]eason ?))\d+)?/?((?<title>[ \w.\-']+?) )?((\(|\.)(?<year>\d{4})(\)|\.?))?(([sS](?<season>\d+)) ?[eE](?<episode>\d+))?((-|\.)(?<fileInfo>[\w\.]*?))?\.(?<extension>\S{3,})";

    /// <summary>
    /// Scann shows
    /// Requirement: S01E01 contained in episode name
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task CreateShow(string path)
    {
        var splitPath = path.Split('/');
        var folderTitle = splitPath
            .SkipWhile(i => i != "Shows") // Skip elements until "Shows" is found
            .Skip(1)                      // Skip "Shows" itself
            .FirstOrDefault();            // Get the next element, or null if none exists

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
        var title = groups["title"].Value;
        var year = groups["yearFolder"].Value;
        int? episodeNr = null;
        int? seasonNr = null;
        if (int.TryParse(groups["episode"].Value, out int episodeNrTemp))
        {
            episodeNr = episodeNrTemp;
        }
        if (int.TryParse(groups["season"].Value, out var seasonNrTemp))
        {
            seasonNr = seasonNrTemp;
        }

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
                year: year,
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
                SeasonNr = seasonNr,
                FolderPath = Directory.GetParent(path)?.FullName ?? Directory.GetCurrentDirectory()
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: season.Id,
                title: show.Title,
                year: year,
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
                Title = $"{folderTitle} S{seasonNr}E{episodeNr}",
                Versions =
                [
                    new()
                    {
                        Id = versionId,
                        Path = path,
                        FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Episode"))?.Id
                    }
                ],
                EpisodeNr = episodeNr,
                SeasonNr = season.SeasonNr
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: episode.Id,
                title: show.Title,
                year: year,
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