using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class FavoriteEndpoints(ILogger<FavoriteEndpoints> logger, IInventoryService inventoryService, IDataRepository dataRepository) : IFavoriteEndpoints
{
    private readonly ILogger<FavoriteEndpoints> _logger = logger;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/favorite");

        group.MapPost("", Favorite).RequireAuthorization();
        group.MapDelete("", Unfavorite).RequireAuthorization();
        group.MapGet("/category/{category}", ListAllInCategory).RequireAuthorization();
        group.MapGet("", IsFavorited).RequireAuthorization();
        group.MapGet("/batch", IsFavoritedBatch).RequireAuthorization();
    }

    public async Task<IResult> Favorite(HttpContext httpContext, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var item = await inventoryService.GetItem<InventoryItem>(inventoryItemId);
        if (item == null)
        {
            return Results.NotFound();
        }

        var favorites = await dataRepository.ListObjectsAsync<FavoriteInfo>(fi => fi.UserId == userId && fi.InventoryId == inventoryItemId);

        if (favorites.All(fi => fi.InventoryId != inventoryItemId))
        {
            await dataRepository.WriteObjectAsync(new FavoriteInfo(){Id = Guid.NewGuid(), InventoryId = inventoryItemId, UserId = userId, Category = category});
        }

        return Results.Ok();
    }

    public async Task<IResult> Unfavorite(HttpContext httpContext, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var item = await inventoryService.GetItem<InventoryItem>(inventoryItemId);
        if (item == null)
        {
            return Results.NotFound();
        }

        var favorite = (await dataRepository.ListObjectsAsync<FavoriteInfo>(fi => fi.UserId == userId && fi.InventoryId == inventoryItemId)).FirstOrDefault();

        if (favorite != null)
        {
            await dataRepository.DeleteObjectAsync(favorite);
        }

        return Results.Ok();
    }

    public async Task<IResult> ListAllInCategory(HttpContext httpContext, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var favorites = await dataRepository.ListObjectsAsync<FavoriteInfo>(f => f.Category == category && f.UserId == userId);;

        return Results.Ok(favorites.Select(f => f.InventoryId));
    }

    public async Task<IResult> IsFavorited(HttpContext httpContext, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var favorites = (await dataRepository.ListObjectsAsync<FavoriteInfo>(f => f.UserId == userId && f.InventoryId == inventoryItemId)).FirstOrDefault();

        return Results.Ok(favorites != null);
    }

    public async Task<IResult> IsFavoritedBatch(HttpContext httpContext, [FromQuery] Guid[] ids, [FromQuery] string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var favorites = await dataRepository.ListObjectsAsync<FavoriteInfo>(f => f.UserId == userId && ids.Contains(f.InventoryId));

        var result = ids.ToDictionary(id => id, id => favorites.Any(f => f.InventoryId == id));

        return Results.Ok(result);
    }
}