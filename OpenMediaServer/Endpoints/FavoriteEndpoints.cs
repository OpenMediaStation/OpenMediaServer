using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class FavoriteEndpoints(ILogger<FavoriteEndpoints> logger, IInventoryService inventoryService, IFileSystemRepository fileSystemRepository) : IFavoriteEndpoints
{
    private readonly ILogger<FavoriteEndpoints> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IFileSystemRepository _fileSystemRepository = fileSystemRepository;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/favorite");

        group.MapPost("", Favorite).RequireAuthorization();
        group.MapDelete("", Unfavorite).RequireAuthorization();
        group.MapGet("/category/{category}", ListAllInCategory).RequireAuthorization();
        group.MapGet("", IsFavorited).RequireAuthorization();
    }

    public async Task<IResult> Favorite(HttpContext httpContext, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var item = await _inventoryService.GetItem<InventoryItem>(inventoryItemId, category);
        if (item == null)
        {
            return Results.NotFound();
        }

        var path = GetFavoritesFilePath(userId, category);
        var favorites = await _fileSystemRepository.ReadObject<List<Guid>>(path) ?? [];

        if (!favorites.Contains(inventoryItemId))
        {
            favorites.Add(inventoryItemId);
            await _fileSystemRepository.WriteObject(path, favorites);
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

        var item = await _inventoryService.GetItem<InventoryItem>(inventoryItemId, category);
        if (item == null)
        {
            return Results.NotFound();
        }

        var path = GetFavoritesFilePath(userId, category);
        var favorites = await _fileSystemRepository.ReadObject<List<Guid>>(path) ?? [];

        if (favorites.Contains(inventoryItemId))
        {
            favorites.Remove(inventoryItemId);
            await _fileSystemRepository.WriteObject(path, favorites);
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

        var path = GetFavoritesFilePath(userId, category);
        var favorites = await _fileSystemRepository.ReadObject<List<Guid>>(path) ?? [];

        return Results.Ok(favorites);
    }

    public async Task<IResult> IsFavorited(HttpContext httpContext, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var path = GetFavoritesFilePath(userId, category);
        var favorites = await _fileSystemRepository.ReadObject<List<Guid>>(path);

        return Results.Ok(favorites?.Contains(inventoryItemId) ?? false);
    }

    private string GetFavoritesFilePath(string userId, string category)
    {
        return Path.Combine(Globals.GetUserStorage(userId), "favorites", category) + ".json";
    }
}