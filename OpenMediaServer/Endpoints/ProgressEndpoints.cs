using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Endpoints;

public class ProgressEndpoints : IProgressEndpoints
{
    private readonly ILogger<ProgressEndpoints> _logger;
    private readonly IProgressService _progressService;

    public ProgressEndpoints(ILogger<ProgressEndpoints> logger, IProgressService progressService)
    {
        _logger = logger;
        _progressService = progressService;
    }

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/progress").RequireAuthorization();

        group.MapGet("", GetProgress);
        group.MapGet("list", ListProgresses);
        group.MapPost("", UpdateProgress);
    }

    public async Task<IResult> UpdateProgress(HttpContext httpContext, Progress progress)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        if (progress.Category == null)
        {
            return Results.BadRequest("Category must be set");
        }

        await _progressService.UpdateProgress(progress, userId);

        return Results.NotFound();
    }

    public async Task<IResult> GetProgress(HttpContext httpContext, string category, Guid? progressId, Guid? parentId)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        try
        {
            var progress = await _progressService.GetProgress(userId, category, progressId, parentId);

            if (progress == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(progress);
        }
        catch (ArgumentNullException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    public async Task<IResult> ListProgresses(HttpContext httpContext, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var progresses = await _progressService.ListProgresses(userId, category);

        if (progresses == null)
        {
            return Results.Ok(new List<Progress>());
        }

        return Results.Ok(progresses);
    }
}
