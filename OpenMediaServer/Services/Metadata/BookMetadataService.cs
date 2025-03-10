using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class BookMetadataService : IBookMetadataService
{
    private readonly IGoogleBooksApi _googleBooksApi;

    public BookMetadataService(IGoogleBooksApi googleBooksApi)
    {
        _googleBooksApi = googleBooksApi;
    }

    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var result = await _googleBooksApi.GetBookMetadata
        (
            title: title
        );

        var data = result?.Items?.FirstOrDefault()?.VolumeInfo;

        var metadata = new MetadataModel()
        {
            Title = data?.Title,
            Book = new()
            {
                Authors = data?.Authors,
                Publisher = data?.Publisher,
                PublishedDate = data?.PublishedDate,
                Description = data?.Description,
                PageCount = data?.PageCount,
                Language = data?.Language,
                Thumbnail = data?.ImageLinks?.Thumbnail
            }
        };

        return metadata;
    }
}
