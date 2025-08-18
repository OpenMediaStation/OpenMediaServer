using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Models;

namespace OpenMediaServer.Extensions.Mapping;

public static class BookmarkExtensions
{
    public static BookmarkDto ToDto(this Bookmark bookmark)
    {
        var result = new BookmarkDto()
        {
            Id = bookmark.Id,
            PositionInSeconds = bookmark.PositionInSeconds,
            Title = bookmark.Title,
            Description = bookmark.Description,
            PageNumber = bookmark.PageNumber,
        };
        
        return result;
    }
    
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