using System;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Interfaces.Services;

public interface IProgressService
{
    Task<Progress?> GetProgress(string userId, string category, Guid? progressId = null, Guid? parentId = null);
    Task<IEnumerable<Progress>?> ListProgresses(string userId, string category);
    Task UpdateProgress(Progress progress, string userId);
    Task CreateProgress(string userId, string category, Guid? parentId);
}
