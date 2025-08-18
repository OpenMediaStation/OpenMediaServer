using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.DTOs.Endpoints.Inventory;
using OpenMediaServer.Extensions.Mapping;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Endpoints;

public class InventoryEndpoints(ILogger<InventoryEndpoints> logger, IInventoryService inventoryService, IContentDiscoveryService contentDiscovery, IVersionService versionService, IAddonService addonService, IPartService partService) : IInventoryEndpoints
{
    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/inventory").RequireAuthorization();

        group.MapGet("/movie", GetMovie);
        group.MapGet("/show", GetShow);
        group.MapGet("/show/batch", GetShows);
        group.MapGet("/episode", GetEpisode);
        group.MapGet("/episode/batch", GetEpisodes);
        group.MapGet("/season", GetSeason);
        group.MapGet("/season/batch", GetSeasons);
        group.MapGet("/book", GetBook);
        group.MapGet("/book/batch", GetBooks);
        group.MapGet("/audiobook", GetAudiobook);
        group.MapGet("/audiobook/batch", GetAudiobooks);

        group.MapGet("/categories", ListCategories);
        group.MapGet("/items", ListItems);
        group.MapGet("/item", GetItem);

        group.MapPost("/rescan", Rescan);
    }

    public async Task<IResult> Rescan()
    {
        await contentDiscovery.ActiveScan(Globals.MediaFolder);

        return Results.Ok();
    }

    public async Task<IResult> GetMovie(Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
        }
        else
        {
            return Results.NotFound("Id not found in movies");
        }
    }

    public async Task<IResult> GetShow(Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
        }
        else
        {
            return Results.NotFound("Id not found in shows");
        }
    }

    public async Task<IResult> GetShows([FromQuery] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return Results.BadRequest("Invalid or missing episode IDs.");
        }

        var items = new List<InventoryItemDto>();

        foreach (var id in ids)
        {
            var item = await inventoryService.GetItem(id: id);
            if (item != null)
            {
                items.Add(await ToDto(item));
            }
        }

        return Results.Ok(items);
    }

    public async Task<IResult> GetEpisode(Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
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

        var items = new List<InventoryItemDto>();

        foreach (var id in ids)
        {
            var item = await inventoryService.GetItem(id: id);
            if (item != null)
            {
                items.Add(await ToDto(item));
            }
        }

        return Results.Ok(items);
    }

    public async Task<IResult> GetSeason(Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
        }
        else
        {
            return Results.NotFound("Id not found in seasons");
        }
    }

    public async Task<IResult> GetSeasons([FromQuery] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return Results.BadRequest("Invalid or missing season IDs.");
        }

        var items = new List<InventoryItemDto>();

        foreach (var id in ids)
        {
            var item = await inventoryService.GetItem(id: id);
            if (item != null)
            {
                items.Add(await ToDto(item));
            }
        }

        return Results.Ok(items);
    }

    public async Task<IResult> GetBook(Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
        }
        else
        {
            return Results.NotFound("Id not found in seasons");
        }
    }

    public async Task<IResult> GetBooks([FromQuery] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return Results.BadRequest("Invalid or missing season IDs.");
        }

        var items = new List<InventoryItemDto>();

        foreach (var id in ids)
        {
            var item = await inventoryService.GetItem(id: id);
            if (item != null)
            {
                items.Add(await ToDto(item));
            }
        }

        return Results.Ok(items);
    }

    public async Task<IResult> GetAudiobook(Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
        }
        else
        {
            return Results.NotFound("Id not found in seasons");
        }
    }

    public async Task<IResult> GetAudiobooks([FromQuery] Guid[] ids)
    {
        if (ids == null || !ids.Any())
        {
            return Results.BadRequest("Invalid or missing season IDs.");
        }

        var items = new List<InventoryItemDto>();

        foreach (var id in ids)
        {
            var item = await inventoryService.GetItem(id: id);
            if (item != null)
            {
                items.Add(await ToDto(item));
            }
        }

        return Results.Ok(items);
    }

    /// <summary>
    /// List all categories available
    /// </summary>
    /// <returns></returns>
    public IResult ListCategories()
    {
        var categories = inventoryService.ListCategories();

        return Results.Ok(categories);
    }

    public async Task<IResult> ListItems(string category)
    {
        var items = await inventoryService.ListItems(category);

        if (items != null && items.Any())
        {
            var itemDtos = new List<InventoryItemDto>();
            
            foreach (var item in items)
            {
                itemDtos.Add(await ToDto(item));
            }
            
            return Results.Ok(items);
        }
        else
        {
            return Results.NotFound("Category not found");
        }
    }

    public async Task<IResult> GetItem(string category, Guid id)
    {
        var item = await inventoryService.GetItem(id: id);

        if (item != null)
        {
            return Results.Ok(await ToDto(item));
        }
        else
        {
            return Results.NotFound("Id not found in category");
        }
    }

    private async Task<InventoryItemDto> ToDto(InventoryItem item)
    {
        var versions = await versionService.List(i => i.InventoryItemId == item.Id);
        var addons = await addonService.ListItems(i => i.InventoryItemId == item.Id);
        
        var parts = new Dictionary<Guid, List<InventoryItemPart>>();
            
        foreach (var version in versions ?? [])
        {
            var part = await partService.ListItems(i => i.InventoryItemVersionId == version.Id);
                
            parts.Add(version.Id, part.ToList());
        }
                
        return item.ToDto(versions, addons, parts);
    }
}
