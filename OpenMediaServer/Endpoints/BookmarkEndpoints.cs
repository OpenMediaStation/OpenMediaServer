using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class BookmarkEndpoints(ILogger<BookmarkEndpoints> logger, IInventoryService inventoryService, IDataRepository dataRepository) : IBookmarkEndpoints
{
    private readonly ILogger<BookmarkEndpoints> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IDataRepository _dataRepository = dataRepository;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/bookmark");

        group.MapPost("{inventoryItemId}", AddBookmark).RequireAuthorization();
        group.MapDelete("{inventoryItemId}/{id}", RemoveBookmark).RequireAuthorization();
        group.MapPut("{inventoryItemId}/{id}", UpdateBookmark).RequireAuthorization();
        group.MapGet("{inventoryItemId}/category/{category}", ListAllInCategory).RequireAuthorization();
        group.MapGet("{inventoryItemId}/{id}", GetBookmark).RequireAuthorization();
    }

    public async Task<IResult> AddBookmark(HttpContext httpContext, Guid inventoryItemId, string category, [FromBody] Bookmark bookmark)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var item = await _inventoryService.GetItem<InventoryItem>(inventoryItemId);
        if (item == null)
        {
            return Results.NotFound();
        }

        var entry = new Bookmark
        {
            Id = Guid.NewGuid(),
            PositionInSeconds = bookmark.PositionInSeconds,
            Title = bookmark.Title,
            Description = bookmark.Description,
            PageNumber = bookmark.PageNumber,
            UserId = userId,
            Category = category,
            InventoryItemId = inventoryItemId,
        };
        await _dataRepository.WriteObjectAsync(entry);

        return Results.Ok(entry);
    }

    public async Task<IResult> RemoveBookmark(HttpContext httpContext, Guid id, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }
        await _dataRepository.DeleteObjectAsync<Bookmark>(id);
       
        return Results.Ok();
    }

    public async Task<IResult> UpdateBookmark(HttpContext httpContext, Guid id, [FromBody] Bookmark bookmark, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        bookmark.Id = id;
        if (bookmark.UserId == null)
        {
            bookmark.UserId = userId;
        }
        else if (bookmark.UserId != userId)
        {
            return Results.Forbid();
        }

        await _dataRepository.WriteObjectAsync(bookmark);

        return Results.Ok(bookmark);
    }

    public async Task<IResult> ListAllInCategory(HttpContext httpContext, string category, Guid inventoryItemId)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }
        var bookmarks = await _dataRepository.ListObjectsAsync<Bookmark>(bm => bm.UserId == userId && bm.Category == category && bm.InventoryItemId == inventoryItemId);

        return Results.Ok(bookmarks);
    }

    public async Task<IResult> GetBookmark(HttpContext httpContext, Guid id, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var bookmark = await _dataRepository.GetObjectByIdAsync<Bookmark>(id);
        
        if(bookmark != null && bookmark?.UserId != userId)
            return Results.Forbid();
        
        return bookmark != null ? Results.Ok(bookmark) : Results.NotFound();
    }
}