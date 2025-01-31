using System.Globalization;
using System.Text.RegularExpressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class DiscoveryMovieService(ILogger<DiscoveryMovieService> logger, IFileInfoService fileInfoService, IMetadataService metadataService, IInventoryService inventoryService, IAddonService addonDiscoveryService) : IDiscoveryMovieService
{
    private Format3DRule[] _format3DRules =
    [
        // Kodi rules:
        new Format3DRule(
            precedingToken: "3d",
            token: "hsbs"),

        new Format3DRule(
            precedingToken: "3d",
            token: "sbs"),

        new Format3DRule(
            precedingToken: "3d",
            token: "htab"),

        new Format3DRule(
            precedingToken: "3d",
            token: "tab"),

        // Media Browser rules:
        new Format3DRule("fsbs"),
        new Format3DRule("hsbs"),
        new Format3DRule("sbs"),
        new Format3DRule("ftab"),
        new Format3DRule("htab"),
        new Format3DRule("tab"),
        new Format3DRule("sbs3d"),
        new Format3DRule("mvc")
    ];
    private string[] _subtitleFileExtensions =
    [
        ".ass",
        ".mks",
        ".sami",
        ".smi",
        ".srt",
        ".ssa",
        ".sub",
        ".sup",
        ".vtt"
    ];
    
    private readonly string[] _cleanDateTimeRegex =
    [
        @"(.+[^_\,\.\(\)\[\]\-])[_\.\(\)\[\]\-](19[0-9]{2}|20[0-9]{2})(?![0-9]+|\W[0-9]{2}\W[0-9]{2})([ _\,\.\(\)\[\]\-][^0-9]|).*(19[0-9]{2}|20[0-9]{2})*",
        @"(.+[^_\,\.\(\)\[\]\-])[ _\.\(\)\[\]\-]+(19[0-9]{2}|20[0-9]{2})(?![0-9]+|\W[0-9]{2}\W[0-9]{2})([ _\,\.\(\)\[\]\-][^0-9]|).*(19[0-9]{2}|20[0-9]{2})*"
    ];

    private readonly string[] _cleaningRegexes =
    [
        @"^\s*(?<cleaned>.+?)[ _\,\.\(\)\[\]\-](3d|sbs|tab|hsbs|htab|mvc|HDR|HDC|UHD|UltraHD|4k|ac3|dts|custom|dc|divx|divx5|dsr|dsrip|dutch|dvd|dvdrip|dvdscr|dvdscreener|screener|dvdivx|cam|fragment|fs|hdtv|hdrip|hdtvrip|internal|limited|multi|subs|ntsc|ogg|ogm|pal|pdtv|proper|repack|rerip|retail|cd[1-9]|r5|bd5|bd|se|svcd|swedish|german|read.nfo|nfofix|unrated|ws|telesync|ts|telecine|tc|brrip|bdrip|480p|480i|576p|576i|720p|720i|1080p|1080i|2160p|hrhd|hrhdtv|hddvd|bluray|blu-ray|x264|x265|h264|h265|xvid|xvidvd|xxx|www.www|AAC|DTS|\[.*\])([ _\,\.\(\)\[\]\-]|$)",
        @"^(?<cleaned>.+?)(\[.*\])",
        @"^\s*(?<cleaned>.+?)\WE[0-9]+(-|~)E?[0-9]+(\W|$)",
        @"^\s*\[[^\]]+\](?!\.\w+$)\s*(?<cleaned>.+)",
        @"^\s*(?<cleaned>.+?)\s+-\s+[0-9]+\s*$",
        @"^\s*(?<cleaned>.+?)(([-._ ](trailer|sample))|-(scene|clip|behindthescenes|deleted|deletedscene|featurette|short|interview|other|extra))$"
    ];

    private readonly string _folderRegex = @"^(?<title>[ \w\.\-'`´:：]+?)(?: ?\((?<language>[A-Z][a-zA-Z]+)\) ?)?(?:[\.\( ](?<year>\d{4})[\.\) ]?)?$";
    private readonly string _fileRegex = @"^(?<title>[ \w\.\-'`´:：]+?)(?: ?\((?<language>[A-Z][a-zA-Z]+)\) ?)?(?:[\.\( ](?<year>\d{4})[\.\) ]?)?(?: ?\-(?<version>[\w]+))?(?:\.(?<extension>\w{3,4}))$"; 
    private readonly string _fileRegexWithoutVersion = @"^(?<title>[ \w\.\-'`´:：]+?)(?: ?\((?<language>[A-Z][a-zA-Z]+)\) ?)?(?:[\.\( ](?<year>\d{4})[\.\) ]?)?(?:\.(?<extension>\w{3,4}))$";
    //OldRegex: @"(?<category>(Movies|\w+?))/.*?(?<folderTitle>[ \w.-]*?)?((\(|\.)(?<yearFolder>\d{4})(\)|\.?))?/?(?<title>[ \w.-.']+?)(?:\((?<year>\d{4})\)|\((?<addition>\D+?)\))?(?:\s*[-.]\s*(?<hyphenAddition>[ \w.-]+?))?([-\.](?<fileInfo>[\w.]*?))?\.(?<extension>\S{3,4})$";
    
    public async Task CreateMovie(string path)
    {
        
        var fileRegex = new Regex
        (
            pattern: _fileRegex,
            options: RegexOptions.Compiled
        );
        var fileRegexWithoutVersion = new Regex(
            pattern: _fileRegexWithoutVersion,
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
        
        string? title = null;
        
        var cleaningRegexes = _cleaningRegexes
            .Select(exp => new Regex(exp, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToArray();
        var datetimeCleaningRegexes = _cleanDateTimeRegex.Select(exp => new Regex(exp, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToArray();
        
        if (TryClean(filePart, cleaningRegexes, out var newName))
            title = newName.Replace("."," ");
        if (TryCleanYear(title ?? filePart, datetimeCleaningRegexes, out var newTitle, out var year))
        {
            title = newTitle!.Replace("."," ");
            logger.LogDebug($"MovieYear: {year}");
        }
        
        
        //todo if(fileName matches "part1" or "cd1" or similar. match on folder instead.(for Title))
        
         var match = fileRegex.Match(filePart);
         if (!match.Success)
         {
             match = fileRegexWithoutVersion.Match(filePart);
             if (!match.Success && title == null)
             {
                logger.LogWarning("Invalid path: {Path}", path);
                return;
             }
         }
        
         var folderMatch = folderRegex.Match(folderPart);
        
        var fileGroups = match.Groups;
        var folderGroups = folderMatch.Groups;
        
        var category = "Movie";
        var folderTitle = folderGroups["title"].Value;
        title = !String.IsNullOrEmpty(folderTitle) && filePart.StartsWith(folderTitle) ? folderTitle.Trim() : title?.Trim() ?? fileGroups["title"].Value.Replace("."," ").Trim();
        var versionName = fileGroups["version"].Value;
        
        logger.LogDebug("Movie detected");

        var movies = await inventoryService.ListItems<Movie>("Movie");
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
                version.FileInfoId = (await fileInfoService.CreateFileInfo(path, version.Id, category))?.Id;

                existingMovie.Versions = existingMovie.Versions?.Append(version);

                var addons = addonDiscoveryService.DiscoverAddons(path);
                existingMovie.Addons = existingMovie.Addons?.Concat(addons);

                await inventoryService.UpdateByTitle(existingMovie);
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
                    FileInfoId = (await fileInfoService.CreateFileInfo(path, versionId, category))?.Id
                }
            ],
            Title = //!string.IsNullOrEmpty(hypenAddition) && folderPath != null ? $"{title} - {hypenAddition}" :
                title,
            FolderPath = folderPath,
            Addons = addonDiscoveryService.DiscoverAddons(path)
        };

        var metadata = await metadataService.CreateNewMetadata
        (
            parentId: movie.Id,
            title: movie.Title,
            category: movie.Category,
            year: (year?.ToString()) ?? (fileGroups.TryGetValue("year", out var movieTitleYear) ? movieTitleYear.Value :
                fileGroups.TryGetValue("folderYear", out var movieFolderYear) ? movieFolderYear.Value : null)
        );

        movie.MetadataId = metadata?.Id;

        await inventoryService.AddItem(movie);
    }

    private static bool TryCleanYear(string title, IReadOnlyList<Regex> expressions, out string? newTitle, out int? parsedYear)
    {
        foreach (var expression in expressions)
        {
            var match = expression.Match(title);

            if (match.Success
                && match.Groups.Count == 5
                && match.Groups[1].Success
                && match.Groups[2].Success
                && int.TryParse(match.Groups[2].ValueSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year))
            {
                newTitle = match.Groups[1].Value.TrimEnd();
                parsedYear = year;
                
                return true;
            }
        }

        newTitle = title;
        parsedYear = null;
        newTitle = null;
        
        return false;   
    }
    
    /// <summary>
    /// Attempts to extract clean name with regular expressions.
    /// </summary>
    /// <param name="name">Name of file.</param>
    /// <param name="expressions">List of regex to parse name and year from.</param>
    /// <param name="newName">Parsing result string.</param>
    /// <returns>True if parsing was successful.</returns>
    private static bool TryClean(string? name, IReadOnlyList<Regex> expressions, out string newName)
    {
        if (string.IsNullOrEmpty(name))
        {
            newName = string.Empty;
            return false;
        }

        // Iteratively apply the regexps to clean the string.
        bool cleaned = false;
        for (int i = 0; i < expressions.Count; i++)
        {
            if (TryClean(name, expressions[i], out newName))
            {
                cleaned = true;
                name = newName;
            }
        }

        newName = cleaned ? name : string.Empty;
        return cleaned;
    }
    
    private static bool TryClean(string name, Regex expression, out string newName)
    {
        var match = expression.Match(name);
        if (match.Success && match.Groups.TryGetValue("cleaned", out var cleaned))
        {
            newName = cleaned.Value;
            return true;
        }

        newName = string.Empty;
        return false;
    }
    
    
    
}


    /// <summary>
    /// Data holder class for 3D format rule.
    /// </summary>
    public class Format3DRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Format3DRule"/> class.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <param name="precedingToken">Token present before current token.</param>
        public Format3DRule(string token, string? precedingToken = null)
        {
            Token = token;
            PrecedingToken = precedingToken;
        }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the preceding token.
        /// </summary>
        /// <value>The preceding token.</value>
        public string? PrecedingToken { get; set; }
    }

