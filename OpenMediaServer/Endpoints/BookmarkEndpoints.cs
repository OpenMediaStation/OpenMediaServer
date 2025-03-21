using Microsoft.AspNetCore.Mvc;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class BookmarkEndpoints(ILogger<BookmarkEndpoints> logger, IInventoryService inventoryService, IFileSystemRepository fileSystemRepository) : IBookmarkEndpoints
{
    private readonly ILogger<BookmarkEndpoints> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IFileSystemRepository _fileSystemRepository = fileSystemRepository;

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

        var item = await _inventoryService.GetItem<InventoryItem>(inventoryItemId, category);
        if (item == null)
        {
            return Results.NotFound();
        }

        var path = GetBookmarksFilePath(userId, category, inventoryItemId);
        var bookmarks = await _fileSystemRepository.ReadObject<List<Bookmark>>(path) ?? [];

        var entry = new Bookmark
        {
            Id = Guid.NewGuid(),
            PositionInSeconds = bookmark.PositionInSeconds,
            Title = bookmark.Title,
            Description = bookmark.Description,
            PageNumber = bookmark.PageNumber
        };

        bookmarks.Add(entry);
        await _fileSystemRepository.WriteObject(path, bookmarks);

        return Results.Ok(entry);
    }

    public async Task<IResult> RemoveBookmark(HttpContext httpContext, Guid id, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var path = GetBookmarksFilePath(userId, category, inventoryItemId);
        var bookmarks = await _fileSystemRepository.ReadObject<List<Bookmark>>(path) ?? [];

        var removed = bookmarks.RemoveAll(b => b.Id == id) > 0;
        await _fileSystemRepository.WriteObject(path, bookmarks);

        return removed ? Results.Ok() : Results.NotFound();
    }

    public async Task<IResult> UpdateBookmark(HttpContext httpContext, Guid id, [FromBody] Bookmark bookmark, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var path = GetBookmarksFilePath(userId, category, inventoryItemId);
        var bookmarks = await _fileSystemRepository.ReadObject<List<Bookmark>>(path) ?? [];

        var entry = bookmarks.FirstOrDefault(b => b.Id == id);
        if (entry == null)
        {
            return Results.NotFound();
        }

        entry.PositionInSeconds = bookmark.PositionInSeconds;
        entry.Title = bookmark.Title;
        entry.Description = bookmark.Description;
        entry.PageNumber = bookmark.PageNumber;

        await _fileSystemRepository.WriteObject(path, bookmarks);

        return Results.Ok(entry);
    }

    public async Task<IResult> ListAllInCategory(HttpContext httpContext, string category, Guid inventoryItemId)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var path = GetBookmarksFilePath(userId, category, inventoryItemId);
        var bookmarks = await _fileSystemRepository.ReadObject<List<Bookmark>>(path) ?? [];

        return Results.Ok(bookmarks);
    }

    public async Task<IResult> GetBookmark(HttpContext httpContext, Guid id, Guid inventoryItemId, string category)
    {
        var userId = Globals.GetUserId(httpContext);
        if (userId == null)
        {
            return Results.Forbid();
        }

        var path = GetBookmarksFilePath(userId, category, inventoryItemId);
        var bookmarks = await _fileSystemRepository.ReadObject<List<Bookmark>>(path) ?? [];

        var bookmark = bookmarks.FirstOrDefault(b => b.Id == id);
        return bookmark != null ? Results.Ok(bookmark) : Results.NotFound();
    }

    private string GetBookmarksFilePath(string userId, string category, Guid inventoryItemId)
    {
        return Path.Combine(Globals.GetUserStorage(userId), "bookmarks", category, inventoryItemId.ToString()) + ".json";
    }
}