using System;
using OpenMediaServer.DTOs;

namespace OpenMediaServer.Interfaces.APIs;

public interface IOpenLibraryApi
{
    Task<OpenLibrarySearchResult?> SearchBook(string title, bool isAudiobook = false, string language = "en");
    string? GetCover(bool isAudiobook, BookDoc? bookData, IEnumerable<BookEdition>? editions);
    Task<OpenLibraryWorkDetails?> GetBookDetails(string? workKey);
    Task<BookEditionsResponse?> GetWorks(string? workKey);
}
