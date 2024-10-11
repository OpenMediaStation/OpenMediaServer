using OpenMediaServer.DTOs;

namespace OpenMediaServer.Interfaces.APIs;

public interface IGoogleBooksApi
{
    Task<GoogleBooksModel?> GetBookMetadata(string title, string? apiKey = null, int? maxResults = 1, string? language = null, string? printType = "books");
}
