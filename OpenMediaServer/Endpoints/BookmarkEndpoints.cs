using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Extensions.Mapping;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class BookmarkEndpoints(ILogger<BookmarkEndpoints> logger, IInventoryService inventoryService, IDataRepository dataRepository) : IBookmarkEndpoints
{
    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/bookmark");

        group.MapPost("{inventoryItemId}", AddBookmark).RequireAuthorization();
        group.MapDelete("{inventoryItemId}/{id}", RemoveBookmark).RequireAuthorization();
        group.MapPut("{inventoryItemId}/{id}", UpdateBookmark).RequireAuthorization();
        group.MapGet("{inventoryItemId}/category/{category}", ListAllInCategory).RequireAuthorization();
        group.MapGet("{inventoryItemId}/{id}", GetBookmark).RequireAuthorization();
    }

    public async Task<IResult> AddBookmark(HttpContext httpContext, Guid inventoryItemId, string category, [FromBody] BookmarkDto bookmark)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var item = await inventoryService.GetItem(inventoryItemId);
        if (item == null)
        {
            return Results.NotFound();
        }

        var entry = bookmark.ToTable(userId, category, inventoryItemId);
        entry.Id = Guid.NewGuid();
        
        await dataRepository.WriteObject(entry);

        return Results.Ok(entry);
    }

    public async Task<IResult> RemoveBookmark(HttpContext httpContext, Guid id, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }
        await dataRepository.DeleteObjectWithFilter<Bookmark>(id);
       
        return Results.Ok();
    }

    public async Task<IResult> UpdateBookmark(HttpContext httpContext, Guid id, [FromBody] BookmarkDto bookmarkDto, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var bookmark = bookmarkDto.ToTable(userId, category, inventoryItemId);

        bookmark.Id = id;
        if (bookmark.UserId == null)
        {
            bookmark.UserId = userId;
        }
        else if (bookmark.UserId != userId)
        {
            return Results.Forbid();
        }

        await dataRepository.WriteObject(bookmark);

        return Results.Ok(bookmark);
    }

    public async Task<IResult> ListAllInCategory(HttpContext httpContext, string category, Guid inventoryItemId)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }
        var bookmarks = await dataRepository.ListObjects<Bookmark>(bm => bm.UserId == userId && bm.Category == category && bm.InventoryItemId == inventoryItemId);

        List<BookmarkDto> bookmarkDtos = [];
        
        foreach (var bookmark in bookmarks)
        {
            bookmarkDtos.Add(bookmark.ToDto());
        }
        
        return Results.Ok(bookmarkDtos);
    }

    public async Task<IResult> GetBookmark(HttpContext httpContext, Guid id, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var bookmark = await dataRepository.GetObjectById<Bookmark>(id);
        
        if(bookmark != null && bookmark?.UserId != userId)
            return Results.Forbid();
        
        return bookmark != null ? Results.Ok(bookmark.ToDto()) : Results.NotFound();
    }
}