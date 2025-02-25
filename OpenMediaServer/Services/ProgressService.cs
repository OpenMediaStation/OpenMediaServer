using System;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Services;

public class ProgressService : IProgressService
{
    private readonly ILogger<ProgressService> _logger;
    private readonly IFileSystemRepository _fileSystemRepository;
    private readonly IInventoryService _inventoryService;

    public ProgressService(ILogger<ProgressService> logger, IFileSystemRepository fileSystemRepository, IInventoryService inventoryService)
    {
        _logger = logger;
        _fileSystemRepository = fileSystemRepository;
        _inventoryService = inventoryService;
    }

    public async Task CreateProgress(string userId, Progress newProgress)
    {
        if (newProgress.ParentId == null)
        {
            throw new ArgumentNullException("parentId");
        }

        if (newProgress.Category == null)
        {
            throw new ArgumentNullException("category");
        }

        var path = GetProgressFilePath(userId, newProgress.Category);
        List<Progress> progresses = await _fileSystemRepository.ReadObject<List<Progress>>(path) ?? [];

        var progress = new Progress()
        {
            Id = Guid.NewGuid(),
            ParentId = newProgress.ParentId,
            Category = newProgress.Category,
            ProgressPercentage = newProgress.ProgressPercentage ?? 0,
            ProgressSeconds = newProgress.ProgressSeconds ?? 0,
            Completions = newProgress.Completions,
        };

        progresses.Add(progress);

        await _fileSystemRepository.WriteObject(path, progresses);
    }

    public async Task UpdateProgress(Progress progress, string userId)
    {
        if (progress.Category == null)
        {
            throw new ArgumentNullException("progress.Category");
        }

        if (progress.ParentId == null)
        {
            throw new ArgumentNullException("progress.ParentId");
        }

        var path = GetProgressFilePath(userId, progress.Category);
        List<Progress> progresses = await _fileSystemRepository.ReadObject<List<Progress>>(path) ?? [];
        var existingProgress = progresses.FirstOrDefault(i => i.Id == progress.Id);

        if (existingProgress != null)
        {
            progresses.RemoveAll(i => i.Id == progress.Id);

            existingProgress.ProgressPercentage = progress.ProgressPercentage;
            existingProgress.ProgressSeconds = progress.ProgressSeconds;
            existingProgress.Completions = progress.Completions;

            progresses.Add(existingProgress);

            await _fileSystemRepository.WriteObject(path, progresses);
        }
        else
        {
            await CreateProgress(newProgress: progress, userId: userId);
        }

        if (progress.Category == "Episode")
        {
            var episodes = await _inventoryService.ListItems<Episode>("Episode");
            var filteredEpisodes = episodes?.Where(i => i.Id == progress.ParentId);
            var seasonId = filteredEpisodes?.FirstOrDefault()?.SeasonId;
            var episodeIds = episodes?.Where(i => i.SeasonId == seasonId).Select(i => i.Id);

            int episodeCount = episodeIds?.Count() ?? 0;

            var episodeProgresses = await ListProgresses(userId, "Episode");
            episodeProgresses = episodeProgresses?.Where(i => i.ParentId != null && (episodeIds?.Contains(i.ParentId.Value) ?? false));

            var seasonProgress = new Progress();
            seasonProgress.Category = "Season";
            seasonProgress.ParentId = seasonId;

            // Calculate average of all episode completions
            while (episodeProgresses?.Count() < episodeCount)
            {
                episodeProgresses = episodeProgresses.Append(new Progress() { Completions = 0 });
            }
            seasonProgress.Completions = (int)Math.Floor(episodeProgresses?.Select(i => i.Completions).Average(i => i) ?? 0);

            // Get existing season id if existing
            if (seasonId != null)
            {
                var seasonProgresses = await ListProgresses(userId, "Season");
                var existingProgresses = seasonProgresses?.Where(i => i.ParentId == seasonId);
                if (existingProgresses?.Any() ?? false)
                {
                    seasonProgress.Id = existingProgresses.First().Id;
                }
            }

            await UpdateProgress(seasonProgress, userId);
        }
        else if (progress.Category == "Season")
        {
            var seasons = await _inventoryService.ListItems<Season>("Season");
            var filteredSeasons = seasons?.Where(i => i.Id == progress.ParentId);
            var showId = filteredSeasons?.FirstOrDefault()?.ShowId;
            var seasonIds = seasons?.Where(i => i.ShowId == showId).Select(i => i.Id);

            int seasonCount = seasonIds?.Count() ?? 0;

            var seasonProgresses = await ListProgresses(userId, "Season");
            seasonProgresses = seasonProgresses?.Where(i => i.ParentId != null && (seasonIds?.Contains(i.ParentId.Value) ?? false));

            var seasonProgress = new Progress();
            seasonProgress.Category = "Show";
            seasonProgress.ParentId = showId;

            // Calculate average of all episode completions
            while (seasonProgresses?.Count() < seasonCount)
            {
                seasonProgresses = seasonProgresses.Append(new Progress() { Completions = 0 });
            }
            seasonProgress.Completions = (int)Math.Floor(seasonProgresses?.Select(i => i.Completions).Average(i => i) ?? 0);

            // Get existing season id if existing
            if (showId != null)
            {
                var showProgresses = await ListProgresses(userId, "Show");
                var existingProgresses = showProgresses?.Where(i => i.ParentId == showId);
                if (existingProgresses?.Any() ?? false)
                {
                    seasonProgress.Id = existingProgresses.First().Id;
                }
            }

            await UpdateProgress(seasonProgress, userId);
        }
    }

    public async Task<Progress?> GetProgress(string userId, string category, Guid? progressId = null, Guid? parentId = null)
    {
        if ((progressId == null && parentId == null) || (progressId != null && parentId != null))
        {
            throw new ArgumentException("ProgressId or ParentId must be set", "id");
        }

        var path = GetProgressFilePath(userId, category);
        var progresses = await _fileSystemRepository.ReadObject<List<Progress>>(path);

        if (progressId != null)
        {
            var progress = progresses?.FirstOrDefault(i => i.Id == progressId);

            return progress;
        }

        var progressParentFiltered = progresses?.FirstOrDefault(i => i.ParentId == parentId);

        return progressParentFiltered;

    }

    public async Task<IEnumerable<Progress>?> ListProgresses(string userId, string category)
    {
        var path = GetProgressFilePath(userId, category);
        var progresses = await _fileSystemRepository.ReadObject<IEnumerable<Progress>>(path);

        return progresses;
    }

    private string GetProgressFilePath(string userId, string category)
    {
        return Path.Combine(Globals.GetUserStorage(userId), "progress", category) + ".json";
    }
}
