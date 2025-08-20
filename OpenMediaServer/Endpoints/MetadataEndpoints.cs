using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.DTOs.Endpoints.Metadata;
using OpenMediaServer.Extensions.Mapping;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Endpoints;

public class MetadataEndpoints(
    ILogger<MetadataEndpoints> logger,
    IMetadataService metadataService,
    IInventoryService inventoryService,
    IMovieMetadataService movieMetadataService,
    IShowMetadataService showMetadataService,
    ISeasonMetadataService seasonMetadataService,
    IEpisodeMetadataService episodeMetadataService,
    IAudioBookMetadataService audiobookMetadataService,
    IBookMetadataService bookMetadataService,
    IChapterService chapterService)
    : IMetadataEndpoints
{
    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/metadata").RequireAuthorization();

        group.MapGet("list", ListMetadata);
        group.MapGet("", GetMetadata);
        group.MapGet("/batch", GetMetadatas);
        // group.MapPost("", UpdateOrAddMetadata);
    }

    public async Task<IResult> ListMetadata(string category)
    {
        var metadatas = await metadataService.ListMetadata(category);

        var metadataDtos = new List<MetadataDto>();
        
        foreach (var metadata in metadatas)
        {
            metadataDtos.Add(await ToDto(metadata, category));
        }

        return Results.Ok(metadataDtos);
    }

    public async Task<IResult> GetMetadata(string category, Guid id)
    {
        var metadata = await metadataService.GetMetadata(category, id);

        return Results.Ok(await ToDto(metadata, category));
    }

    public async Task<IResult> GetMetadatas(string category, [FromQuery] Guid[] ids)
    {
        var metadataDtos = new List<MetadataDto>();
        
        foreach (var item in ids)
        {
            var metadata = await metadataService.GetMetadata(category, item);
            if (metadata != null)
            {
                metadataDtos.Add(await ToDto(metadata, category));
            }
        }
        
        return Results.Ok(metadataDtos);
    }

    // public async Task<IResult> UpdateOrAddMetadata(MetadataDto metadataModel)
    // {
    //     var success = await metadataService.UpdateOrAddMetadata(metadataModel);
    //
    //     if (success)
    //     {
    //         return Results.Ok(success);
    //     }
    //     else
    //     {
    //         return Results.BadRequest("Could not be added");
    //     }
    // }

    private async Task<MetadataDto> ToDto(MetadataModel metadata, string category)
    {
        var inventoryItem = await inventoryService.GetItem(category, i => i.MetadataId == metadata.Id);
        var movie = await movieMetadataService.Get(metadata.MovieMetadataId);
        var show = await showMetadataService.Get(metadata.ShowMetadataId);
        var season = await seasonMetadataService.Get(metadata.SeasonMetadataId);
        var episode = await episodeMetadataService.Get(metadata.EpisodeMetadataId);
        var book = await bookMetadataService.Get(metadata.BookMetadataId);
        var audiobook = await audiobookMetadataService.Get(metadata.AudiobookMetadataId);
        var chapters = await chapterService.List(i => i.MetadataModelId == metadata.Id);

        return metadata.ToDto(inventoryItem?.Id, movie, show, episode, season, audiobook, book, chapters);
    }
}