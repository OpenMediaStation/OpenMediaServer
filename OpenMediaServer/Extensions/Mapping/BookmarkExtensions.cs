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
}