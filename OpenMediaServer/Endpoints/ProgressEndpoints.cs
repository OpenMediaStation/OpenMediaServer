using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Extensions.Mapping;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Progress;

namespace OpenMediaServer.Endpoints;

public class ProgressEndpoints(ILogger<ProgressEndpoints> logger, IProgressService progressService) : IProgressEndpoints
{
    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/progress").RequireAuthorization();

        group.MapGet("", GetProgress);
        group.MapGet("/batch", GetProgresses);
        group.MapGet("list", ListProgresses);
        group.MapPost("", UpdateProgress);
    }

    public async Task<IResult> UpdateProgress(HttpContext httpContext, ProgressDto progress)
    {
        logger.LogTrace("Updating progress");

        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        if (progress.Category == null)
        {
            return Results.BadRequest("Category must be set");
        }

        await progressService.UpdateProgress(progress.ToTable(), userId);

        return Results.Ok();
    }

    public async Task<IResult> GetProgress(HttpContext httpContext, string category, Guid? progressId, Guid? parentId)
    {
        logger.LogTrace("Getting progress");

        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        try
        {
            var progress = await progressService.GetProgress(userId, category, progressId, parentId);

            if (progress == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(progress.ToDto());
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    public async Task<IResult> GetProgresses(HttpContext httpContext, string category, [FromQuery] Guid[] ids)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        try
        {
            var progresses = new List<ProgressDto>();

            foreach (var id in ids)
            {
                var progress = await progressService.GetProgress(userId, category, null, id);
                if (progress != null)
                {
                    progresses.Add(progress.ToDto());
                }
            }

            return Results.Ok(progresses);
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

        var progresses = await progressService.ListProgresses(userId, category);

        if (progresses == null)
        {
            return Results.Ok(new List<ProgressDto>());
        }

        return Results.Ok(progresses.Select(i => i.ToDto()));
    }
}
