using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.FileInfo;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Discovery;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services.Discovery;

public class DiscoveryShowService(
    ILogger<DiscoveryShowService> _logger,
    IFileInfoService _fileInfoService,
    IMetadataService _metadataService,
    IInventoryService _inventoryService,
    IAddonService _addonService,
    IBinService _binService,
    IVersionService versionService,
    IShowMetadataService showMetadataService,
    ISeasonMetadataService seasonMetadataService,
    IEpisodeMetadataService episodeMetadataService) : IDiscoveryShowService
{
    public async Task CreateShow(string path)
    {
        var splitPath = path.Split('/');
        var folderTitle = splitPath
            .SkipWhile(i => i != "Shows") // Skip elements until "Shows" is found
            .Skip(1) // Skip "Shows" itself
            .FirstOrDefault(); // Get the next element, or null if none exists

        var discoveryInfo = GetInfo(path);

        if (discoveryInfo == null)
            return;

        _logger.LogDebug("Show detected");

        
        var cleanedFolderTitle = !string.IsNullOrWhiteSpace(discoveryInfo.Year) ? folderTitle?.Replace(discoveryInfo.Year, "")?.Replace("()", "")?.Trim() : folderTitle?.Trim();
        // Show
        var showPath = Path.Combine(Globals.MediaFolder, "Shows", cleanedFolderTitle);
        var show = await _inventoryService.GetItem("Show", i => i.FolderPath == showPath);

        if (show == null)
        {
            show = await _binService.GetItem<InventoryItem>(cleanedFolderTitle, "Show");

            if (show == null)
            {
                show = new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    Title = cleanedFolderTitle,
                    Category = "Show",
                };

                var metadata = await _metadataService.CreateNewMetadata
                (
                    parentId: show.Id,
                    title: show.Title,
                    year: discoveryInfo?.Year,
                    category: show.Category
                );
                
                var showMetadata = await showMetadataService.Get(metadata?.ShowMetadataId);

                show.MetadataId = metadata?.Id;
                show.DisplayImageBlurHash = showMetadata?.PosterBlurHash;
                show.ReleaseDate = DateTime.TryParse(showMetadata?.Released, out var dateTime) ? dateTime : null;
            }
            else
            {
                await _binService.RemoveById(show);
            }

            show.FolderPath = Path.Combine(Globals.MediaFolder, "Shows", folderTitle);

            await _inventoryService.AddItem(show);
        }

        var folderPath = Directory.GetParent(path)?.FullName ?? Path.GetDirectoryName(path);
        // Season
        var season = await _inventoryService.GetItem("Season", i => i.FolderPath == folderPath);

        if (season == null)
        {
            if (string.IsNullOrWhiteSpace(discoveryInfo?.SeasonFolder))
            {
                discoveryInfo!.SeasonFolder = $"Season {discoveryInfo?.SeasonNr}";
            }

            season = await _binService.GetItem<InventoryItem>(discoveryInfo!.SeasonFolder, "Season");

            if (season == null)
            {
                season = new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    Category = "Season",

                    ShowId = show.Id,
                    Title = discoveryInfo?.SeasonFolder,
                    SeasonNr = discoveryInfo?.SeasonNr,
                };

                var metadata = await _metadataService.CreateNewMetadata
                (
                    parentId: season.Id,
                    title: show.Title,
                    year: discoveryInfo?.Year,
                    category: season.Category,
                    season: discoveryInfo?.SeasonNr
                );
                
                var seasonMetadata = await seasonMetadataService.Get(metadata?.SeasonMetadataId);

                season.MetadataId = metadata?.Id;
                season.DisplayImageBlurHash = seasonMetadata?.PosterBlurHash;
                season.ReleaseDate = seasonMetadata?.AirDate;
            }
            else
            {
                await _binService.RemoveById(season);
            }

            season.FolderPath = Directory.GetParent(path)?.FullName ?? Directory.GetCurrentDirectory();

            await _inventoryService.AddItem(season);

            await _inventoryService.UpdateOrInsert(show);
        }

        // Episode
        var versions = await versionService.List(i => i.Path == path);
        var episodes = new List<InventoryItem>();

        foreach (var version in versions ?? [])
        {
            var item = await _inventoryService.GetItem(version.InventoryItemId);

            if (item != null)
            {
                episodes.Add(item);
            }
        }

        var episode = episodes.FirstOrDefault();

        if (episode == null)
        {
            var title = $"{folderTitle} S{discoveryInfo?.SeasonNr}E{discoveryInfo?.EpisodeNr}";

            episode = await _binService.GetItem<InventoryItem>(title, "Episode");

            var versionId = Guid.NewGuid();

            if (episode == null)
            {
                episode = new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    Category = "Episode",
                    SeasonId = season.Id,
                    Title = title,
                    EpisodeNr = discoveryInfo?.EpisodeNr,
                    SeasonNr = season.SeasonNr,
                };

                var metadata = await _metadataService.CreateNewMetadata
                (
                    parentId: episode.Id,
                    title: show.Title ?? string.Empty,
                    year: discoveryInfo?.Year,
                    category: episode.Category,
                    episode: episode.EpisodeNr,
                    season: episode.SeasonNr
                );
                
                var episodeMetadata = await episodeMetadataService.Get(metadata?.EpisodeMetadataId);

                episode.MetadataId = metadata?.Id;
                episode.DisplayImageBlurHash = episodeMetadata?.BackdropBlurHash;
                episode.ReleaseDate =
                    DateTime.TryParse(episodeMetadata?.Released, out var dateTime) ? dateTime : null;
            }
            else
            {
                await _binService.RemoveById(episode);
            }

            var addons = _addonService.DiscoverAddons(path);

            await _inventoryService.AddItem(episode);
            
            foreach (var addon in addons)
            {
                addon.InventoryItemId = episode.Id;

                await _addonService.UpdateOrInsert(addon);
            }

            var newVersion = new InventoryItemVersion()
            {
                Id = versionId,
                InventoryItemId = episode.Id,
                Path = path,
                FileInfoId = (await _fileInfoService.CreateFileInfo(path, versionId, "Episode"))?.Id
            };
            
            await versionService.UpdateOrInsert(newVersion);
            
            await _inventoryService.UpdateOrInsert(season);
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
            .Skip(2) // Skip "Shows" itself
            .FirstOrDefault(); // Get the next element, or null if none exists

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
            regex:
            @"(?<category>(Shows)|\w+?)/.*?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<seasonFolder>(([sS]taffel ?)|([Ss]eason ?))\d+)?/?((?<title>[ \w.\-':]+?) )?((\(|\.)(?<year>\d{4})(\)|\.?))?(\(?[sS](?<season>\d+)[ ]?[eE](?<episode>\d+)\)?|\([sS](?<seasonParens>\d+)[/⧸][eE](?<episodeParens>\d+)\)).*?\.(?<extension>\S{3,})",
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