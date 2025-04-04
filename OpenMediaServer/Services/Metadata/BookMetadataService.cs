using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class BookMetadataService : IBookMetadataService
{
    private readonly IGoogleBooksApi _googleBooksApi;
    private readonly IOpenLibraryApi _openLibraryApi;
    private readonly IImageService _imageService;

    public BookMetadataService(IGoogleBooksApi googleBooksApi, IOpenLibraryApi openLibraryApi, IImageService imageService)
    {
        _googleBooksApi = googleBooksApi;
        _openLibraryApi = openLibraryApi;
        _imageService = imageService;
    }

    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var googleBooksResults = await _googleBooksApi.GetBookMetadata
        (
            title: title
        );

        var googleBooksData = googleBooksResults?.Items?.FirstOrDefault()?.VolumeInfo;

           var searchResult = await _openLibraryApi.SearchBook(title, true, language ?? "en");

        var openLibraryData = searchResult?.Docs[0]; // Take the first result as the best match

        var openLibraryBookDetails = await _openLibraryApi.GetBookDetails(openLibraryData?.Key);
        var description = openLibraryBookDetails?.Description ?? openLibraryBookDetails?.Description?.ToString();

        var works = await _openLibraryApi.GetWorks(openLibraryData?.Key);

        string? coverUrl = _openLibraryApi.GetCover(false, openLibraryData, works?.Entries) ?? googleBooksData?.ImageLinks?.Thumbnail;
        var thumbnailBlurHash = await WriteImageAndReturnBlurHash(coverUrl, "cover", category: "Book", metadataId.ToString());

        var metadata = new MetadataModel()
        {
            Title = googleBooksData?.Title,
            Book = new()
            {
                Authors = openLibraryData?.AuthorName ?? googleBooksData?.Authors,
                Publisher = googleBooksData?.Publisher,
                PublishedDate = googleBooksData?.PublishedDate,
                Description = description ?? googleBooksData?.Description,
                PageCount = googleBooksData?.PageCount,
                Language = googleBooksData?.Language,
                Thumbnail = coverUrl != null ? $"{Globals.Domain}/images/Book/{metadataId}/cover" : null,
                ThumbnailBlurHash = thumbnailBlurHash
            }
        };

        return metadata;
    }
    
    private async Task<string?> WriteImageAndReturnBlurHash(string? url, string fileName, string category, string id)
    {
        if (url == null)
            return null;

        var (bytes, imageType) = await _openLibraryApi.GetBytesFromUrlAsync(url);

        imageType = imageType?.Split("/").LastOrDefault();
        
        await _imageService.WriteImage(bytes, url, fileName, category, id, imageType);

        return _imageService.CreateBlurHash(bytes);
    }
}
