using System.Threading.Tasks;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class AudiobookMetadataService : IAudioBookMetadataService
{
    private readonly IGoogleBooksApi _googleBooksApi;
    private readonly IOpenLibraryApi _openLibraryApi;
    private readonly IImageService _imageService;

    public AudiobookMetadataService(IGoogleBooksApi googleBooksApi, IOpenLibraryApi openLibraryApi, IImageService imageService)
    {
        _googleBooksApi = googleBooksApi;
        _openLibraryApi = openLibraryApi;
        _imageService = imageService;
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

        var googleBooksData = googleBooksResult?.Items?.FirstOrDefault()?.VolumeInfo;

        string? coverUrl = _openLibraryApi.GetCover(true, openLibraryData, works?.Entries) ?? googleBooksData?.ImageLinks?.Thumbnail;

        coverUrl = await SaveImage(coverUrl, metadataId.ToString());

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
                Thumbnail = coverUrl
            }
        };

        return metadata;
    }

    private async Task<string?> SaveImage(string? coverUrl, string metadataId)
    {
        if (coverUrl == null)
            return null;

        var (bytes, imageType) = await _openLibraryApi.GetBytesFromUrlAsync(coverUrl);

        imageType = imageType?.Split("/").LastOrDefault();

        return await _imageService.WriteImage(bytes, coverUrl, "cover", "Audiobook", metadataId, imageType: imageType);
    }
}
