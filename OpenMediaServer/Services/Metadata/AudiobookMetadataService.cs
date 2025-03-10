using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class AudiobookMetadataService : IAudioBookMetadataService
{
    private readonly IGoogleBooksApi _googleBooksApi;
    private readonly IOpenLibraryApi _openLibraryApi;

    public AudiobookMetadataService(IGoogleBooksApi googleBooksApi, IOpenLibraryApi openLibraryApi)
    {
        _googleBooksApi = googleBooksApi;
        _openLibraryApi = openLibraryApi;
    }

    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var googleBooksResult = await _googleBooksApi.GetBookMetadata
        (
            title: title
        );

        var searchResult = await _openLibraryApi.SearchBook(title, true, language ??= "en");

        var openLibraryData = searchResult?.Docs[0]; // Take the first result as the best match

        var openLibraryBookDetails = await _openLibraryApi.GetBookDetails(openLibraryData?.Key);
        var description = openLibraryBookDetails?.Description ?? openLibraryBookDetails?.Description?.ToString();

        var works = await _openLibraryApi.GetWorks(openLibraryData?.Key);

        string? coverUrl = _openLibraryApi.GetCover(true, openLibraryData, works?.Entries);

        var googleBooksData = googleBooksResult?.Items?.FirstOrDefault()?.VolumeInfo;

        var metadata = new MetadataModel()
        {
            Title = openLibraryData?.Title ?? googleBooksData?.Title,
            Audiobook = new()
            {
                Authors = openLibraryData?.AuthorName ?? googleBooksData?.Authors,
                Publisher = googleBooksData?.Publisher,
                PublishedDate = googleBooksData?.PublishedDate,
                Description = description ?? googleBooksData?.Description,
                Language = googleBooksData?.Language,
                Thumbnail = coverUrl ?? googleBooksData?.ImageLinks?.Thumbnail
            }
        };

        return metadata;
    }
}
