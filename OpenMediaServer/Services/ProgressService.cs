using System;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Services;

public class ProgressService : IProgressService
{
    private readonly ILogger<ProgressService> _logger;
    private readonly IFileSystemRepository _fileSystemRepository;

    public ProgressService(ILogger<ProgressService> logger, IFileSystemRepository fileSystemRepository)
    {
        _logger = logger;
        _fileSystemRepository = fileSystemRepository;
    }

    public async Task CreateProgress(string userId, string category, Guid? parentId)
    {
        if (parentId == null)
        {
            throw new ArgumentNullException("parentId");
        }

        var path = GetProgressFilePath(userId, category);
        List<Progress> progresses = await _fileSystemRepository.ReadObject<List<Progress>>(path) ?? [];

        var progress = new Progress()
        {
            Id = Guid.NewGuid(),
            ParentId = parentId,
            Category = category,
            ProgressPercentage = 0,
            ProgressSeconds = 0,
            Completions = 0,
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
            await CreateProgress(category: progress.Category, userId: userId, parentId: progress.ParentId);
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
