using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Models;

namespace OpenMediaServer.Extensions.Mapping;

public static class BookmarkDtoExtensions
{
    public static Bookmark ToTable(this BookmarkDto bookmark, string userId, string category, Guid inventoryItemId)
    {
        var result = new Bookmark()
        {
            Id = bookmark.Id,
            PositionInSeconds = bookmark.PositionInSeconds,
            Title = bookmark.Title,
            Description = bookmark.Description,
            PageNumber = bookmark.PageNumber,
            UserId = userId,
            Category = category,
            InventoryItemId = inventoryItemId,
        };
        
        return result;
    }
}