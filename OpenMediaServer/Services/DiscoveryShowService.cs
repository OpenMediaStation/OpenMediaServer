using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Discovery;

namespace OpenMediaServer.Services;

public class DiscoveryShowService(ILogger<DiscoveryShowService> logger, IFileInfoService fileInfoService, IMetadataService metadataService, IInventoryService inventoryService, IAddonService addonService) : IDiscoveryShowService
{
    private readonly ILogger<DiscoveryShowService> _logger = logger;
    private readonly IFileInfoService _fileInfoService = fileInfoService;
    private readonly IMetadataService _metadataService = metadataService;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IAddonService _addonService = addonService;

    public async Task CreateShow(string path)
    {
        var splitPath = path.Split('/');
        var folderTitle = splitPath
            .SkipWhile(i => i != "Shows") // Skip elements until "Shows" is found
            .Skip(1)                      // Skip "Shows" itself
            .FirstOrDefault();            // Get the next element, or null if none exists

        var discoveryInfo = GetInfo(path);

        if (discoveryInfo == null)
            return;

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
                FolderPath = Path.Combine(Globals.MediaFolder, "Shows", folderTitle)
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: show.Id,
                title: show.Title,
                year: discoveryInfo?.Year,
                category: show.Category
            );

            show.MetadataId = metadata?.Id;

            await _inventoryService.AddItem(show);
        }

        // Season
        var season = await _inventoryService.GetItem<Season>("Season", i => i.FolderPath == Directory.GetParent(path)?.FullName);

        if (season == null)
        {
            if (string.IsNullOrWhiteSpace(discoveryInfo?.SeasonFolder))
            {
                discoveryInfo.SeasonFolder = $"Season {discoveryInfo?.SeasonNr}";
            }

            season = new Season
            {
                Id = Guid.NewGuid(),

                ShowId = show.Id,
                Title = discoveryInfo?.SeasonFolder,
                SeasonNr = discoveryInfo?.SeasonNr,
                FolderPath = Directory.GetParent(path)?.FullName ?? Directory.GetCurrentDirectory()
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: season.Id,
                title: show.Title,
                year: discoveryInfo?.Year,
                category: season.Category,
                season: discoveryInfo?.SeasonNr
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
                Title = $"{folderTitle} S{discoveryInfo?.SeasonNr}E{discoveryInfo?.EpisodeNr}",
                Versions =
                [
                    new()
                    {
                        Id = versionId,
                        Path = path,
                        FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Episode"))?.Id
                    }
                ],
                EpisodeNr = discoveryInfo?.EpisodeNr,
                SeasonNr = season.SeasonNr,
                Addons = _addonService.DiscoverAddons(path)
            };

            var metadata = await _metadataService.CreateNewMetadata
            (
                parentId: episode.Id,
                title: show.Title,
                year: discoveryInfo?.Year,
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

    private DiscoveryInfo? GetInfo(string path)
    {
        var info = new DiscoveryInfo();

        info = GetRegexInfo(path);

        info ??= GetLeadingDigitEpisodeInfo(path);

        if (info == null)
        {
            _logger.LogWarning("Path invalid for a Show: {Path}", path);
        }

        return info;
    }

    private DiscoveryInfo? GetLeadingDigitEpisodeInfo(string path)
    {
        var info = new DiscoveryInfo();

        // FolderTitle
        var splitPath = path.Split('/');
        var folderTitle = splitPath
            .SkipWhile(i => i != "Shows") // Skip elements until "Shows" is found
            .Skip(2)                      // Skip "Shows" itself
            .FirstOrDefault();            // Get the next element, or null if none exists

        if (folderTitle == splitPath.LastOrDefault())
        {
            folderTitle = null;
        }

        info.SeasonFolder = folderTitle;

        // EpisodeNr
        var matchEpisodeNr = MatchRegex
        (
            regex: @"(?<=[\/])\d*(?=[\.])",
            path: path
        );

        if (matchEpisodeNr == null)
        {
            return null;
        }

        if (int.TryParse(matchEpisodeNr.Value, out int episodeNrTemp))
        {
            info.EpisodeNr = episodeNrTemp;
        }

        // SeasonNr
        if (info.SeasonFolder != null)
        {
            var matchSeasonNr = MatchRegex
            (
                regex: @"(?<=[ ])\d*",
                path: info.SeasonFolder
            );

            if (matchSeasonNr == null)
            {
                return null;
            }

            if (int.TryParse(matchSeasonNr.Value, out int seasonNrNrTemp))
            {
                info.SeasonNr = seasonNrNrTemp;
            }
        }
        else
        {
            // If no season folder exists assume its Season 1
            info.SeasonNr = 1;
        }

        // Validate info
        if (info.EpisodeNr == null || info.SeasonNr == null)
        {
            return null;
        }

        return info;
    }

    private DiscoveryInfo? GetRegexInfo(string path)
    {
        var info = new DiscoveryInfo();

        var match = MatchRegex
        (
            regex: @"(?<category>(Shows)|\w+?)/.*?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<seasonFolder>(([sS]taffel ?)|([Ss]eason ?))\d+)?/?((?<title>[ \w.\-':]+?) )?((\(|\.)(?<year>\d{4})(\)|\.?))?(\(?[sS](?<season>\d+)[ ]?[eE](?<episode>\d+)\)?|\([sS](?<seasonParens>\d+)[/⧸][eE](?<episodeParens>\d+)\)).*?\.(?<extension>\S{3,})",
            path: path
        );

        if (match == null)
        {
            return null;
        }

        var groups = match.Groups;

        info.Year = groups["yearFolder"].Value;
        info.SeasonFolder = groups["seasonFolder"].Value;

        if (int.TryParse(groups["episode"].Value, out int episodeNrTemp))
        {
            info.EpisodeNr = episodeNrTemp;
        }
        else if (int.TryParse(groups["episodeParens"].Value, out int episodeParensNrTemp))
        {
            info.EpisodeNr = episodeParensNrTemp;
        }
        if (int.TryParse(groups["season"].Value, out var seasonNrTemp))
        {
            info.SeasonNr = seasonNrTemp;
        }
        else if (int.TryParse(groups["seasonParens"].Value, out int seasonParensNrTemp))
        {
            info.SeasonNr = seasonParensNrTemp;
        }

        return info;
    }

    private Match? MatchRegex(string regex, string path)
    {
        var pathRegex = new Regex
        (
            pattern: regex,
            options: RegexOptions.Compiled
        );

        var match = pathRegex.Match(path.Replace(Globals.MediaFolder, string.Empty));
        if (!match.Success)
        {
            return null;
        }

        return match;
    }
}