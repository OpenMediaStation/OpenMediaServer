using System;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Services;

public class ProgressService(
    ILogger<ProgressService> logger,
    IDataRepository dataRepository,
    IInventoryService inventoryService)
    : IProgressService
{
    private readonly ILogger<ProgressService> _logger = logger;

    public async Task CreateProgress(string userId, Progress newProgress)
    {
        if (newProgress.ParentId == null)
        {
            throw new ArgumentNullException($"{nameof(newProgress)}.ParentId");
        }

        if (newProgress.Category == null)
        {
            throw new ArgumentNullException($"{nameof(newProgress)}.Category");
        }
        
        if(string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        
        var progress = new Progress()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ParentId = newProgress.ParentId,
            Category = newProgress.Category,
            ProgressPercentage = newProgress.ProgressPercentage ?? 0,
            ProgressSeconds = newProgress.ProgressSeconds ?? 0,
            Completions = newProgress.Completions,
        };
        
        await dataRepository.WriteObject(progress);
    }

    public async Task UpdateProgress(Progress progress, string userId)
    {
        if (progress.Category == null)
        {
            throw new ArgumentNullException($"{nameof(progress)}.Category");
        }

        if (progress.ParentId == null)
        {
            throw new ArgumentNullException($"{nameof(progress)}.ParentId");
        }

        if(string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        
        var existingProgress  = progress.Id == null ? null : await dataRepository.GetObjectById<Progress>((Guid)progress.Id!);

        if (existingProgress != null)
        {
            existingProgress.ProgressPercentage = progress.ProgressPercentage;
            existingProgress.ProgressSeconds = progress.ProgressSeconds;
            existingProgress.Completions = progress.Completions;
            
            await dataRepository.WriteObject(existingProgress);
        }
        else
        {
            await CreateProgress(newProgress: progress, userId: userId);
        }

        if (progress.Category == "Episode") //TODO CHECK AND maybe REFACTOR
        {
            var episodes = await inventoryService.ListItems<Episode>("Episode");
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
            var seasons = await inventoryService.ListItems<Season>("Season");
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
        
        if (progressId != null)
        {
            var progress = await dataRepository.GetObjectById<Progress>((Guid)progressId, p => p.Category == category);

            return progress;
        }

        var progressParentFiltered = (await dataRepository.ListObjects<Progress>(p => p.ParentId == parentId)).FirstOrDefault();

        return progressParentFiltered;
    }

    public async Task<IEnumerable<Progress>?> ListProgresses(string userId, string category)
    {
        var progresses = await dataRepository.ListObjects<Progress>();

        return progresses;
    }

    private string GetProgressFilePath(string userId, string category)
    {
        return Path.Combine(Globals.GetUserStorage(userId), "progress", category) + ".json";
    }
}
