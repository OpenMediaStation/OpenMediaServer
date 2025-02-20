using System;
using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Endpoints;

public class MetadataEndpoints : IMetadataEndpoints
{
    private readonly ILogger<MetadataEndpoints> _logger;
    private readonly IMetadataService _metadataService;

    public MetadataEndpoints(ILogger<MetadataEndpoints> logger, IMetadataService metadataService)
    {
        _logger = logger;
        _metadataService = metadataService;
    }

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/metadata").RequireAuthorization();

        group.MapGet("list", ListMetadata);
        group.MapGet("", GetMetadata);
        group.MapGet("/batch", GetMetadatas);
        group.MapPost("", UpdateOrAddMetadata);
    }

    public async Task<IResult> ListMetadata(string category)
    {
        var metadatas = await _metadataService.ListMetadata(category);

        return Results.Ok(metadatas);
    }

    public async Task<IResult> GetMetadata(string category, Guid id)
    {
        var metadata = await _metadataService.GetMetadata(category, id);

        return Results.Ok(metadata);
    }

    public async Task<IResult> GetMetadatas(string category, [FromQuery] Guid[] ids)
    {
        var metadatas = new List<MetadataModel>();

        foreach (var item in ids)
        {
            var metadata = await _metadataService.GetMetadata(category, item);
            if (metadata != null)
            {
                metadatas.Add(metadata);
            }
        }

        return Results.Ok(metadatas);
    }

    public async Task<IResult> UpdateOrAddMetadata(MetadataModel metadataModel)
    {
        var success = await _metadataService.UpdateOrAddMetadata(metadataModel);

        if (success)
        {
            return Results.Ok(success);
        }
        else
        {
            return Results.BadRequest("Could not be added");
        }
    }
}
