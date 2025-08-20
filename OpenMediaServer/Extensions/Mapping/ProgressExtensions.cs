using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Extensions.Mapping;

public static class ProgressExtensions
{
    public static ProgressDto ToDto(this Progress progress)
    {
        var result = new ProgressDto()
        {
           Id = progress.Id,
           Category = progress.Category,
           ParentId = progress.ParentId,
           ProgressPercentage = progress.ProgressPercentage,
           ProgressSeconds = progress.ProgressSeconds,
           Completions = progress.Completions,
        };

        return result;
    }    
    
    public static Progress ToTable(this ProgressDto progress)
    {
        var result = new Progress()
        {
           Id = progress.Id,
           Category = progress.Category,
           ParentId = progress.ParentId,
           ProgressPercentage = progress.ProgressPercentage,
           ProgressSeconds = progress.ProgressSeconds,
           Completions = progress.Completions,
        };

        return result;
    }

}