using System.Collections;
using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class InventoryEndpoints(ILogger<InventoryEndpoints> logger, IInventoryService inventoryService, IContentDiscoveryService contentDiscovery) : IInventoryEndpoints
{
    private readonly ILogger<InventoryEndpoints> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IContentDiscoveryService _contentDiscovery = contentDiscovery;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/inventory").RequireAuthorization();

        group.MapGet("/movie", GetMovie);
        group.MapGet("/show", GetShow);
        group.MapGet("/episode", GetEpisode);
        group.MapGet("/episode/batch", GetEpisodes);
        group.MapGet("/season", GetSeason);
        group.MapGet("/book", GetBook);

        group.MapGet("/categories", ListCategories);
        group.MapGet("/items", ListItems);
        group.MapGet("/item", GetItem);

        group.MapPost("/rescan", Rescan);
    }

    public async Task<IResult> Rescan()
    {
        await _contentDiscovery.ActiveScan(Globals.MediaFolder);

        return Results.Ok();
    }

    public async Task<IResult> GetMovie(Guid id)
    {
        var item = await _inventoryService.GetItem<Movie>(id: id, category: "Movie");

        if (item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in movies");
        }
    }

    public async Task<IResult> GetShow(Guid id)
    {
        var item = await _inventoryService.GetItem<Show>(id: id, category: "Show");

        if (item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in shows");
        }
    }

    public async Task<IResult> GetEpisode(Guid id)
    {
        var item = await _inventoryService.GetItem<Episode>(id: id, category: "Episode");

        if (item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in episodes");
        }
    }

    public async Task<IResult> GetEpisodes([FromQuery] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return Results.BadRequest("Invalid or missing episode IDs.");
        }

        var items = new List<Episode>();

        foreach (var id in ids)
        {
            var item = await _inventoryService.GetItem<Episode>(id: id, category: "Episode");
            if (item != null)
            {
                items.Add(item);
            }
        }

        return Results.Ok(items);
    }

    public async Task<IResult> GetSeason(Guid id)
    {
        var item = await _inventoryService.GetItem<Season>(id: id, category: "Season");

        if (item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in seasons");
        }
    }

    public async Task<IResult> GetBook(Guid id)
    {
        var item = await _inventoryService.GetItem<Book>(id: id, category: "Book");

        if (item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in seasons");
        }
    }


    /// <summary>
    /// List all categories available
    /// </summary>
    /// <returns></returns>
    public IResult ListCategories()
    {
        var categories = _inventoryService.ListCategories();

        return Results.Ok(categories);
    }

    public async Task<IResult> ListItems(string category)
    {
        var items = await _inventoryService.ListItems<InventoryItem>(category);

        if (items != null && items.Any())
        {
            return Results.Ok(items);
        }
        else
        {
            return Results.NotFound("Category not found");
        }
    }

    public async Task<IResult> GetItem(string category, Guid id)
    {
        var item = await _inventoryService.GetItem<InventoryItem>(id: id, category: category);

        if (item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in category");
        }
    }
}
