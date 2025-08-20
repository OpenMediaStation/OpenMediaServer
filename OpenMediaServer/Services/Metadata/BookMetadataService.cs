using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class BookMetadataService(
    IGoogleBooksApi googleBooksApi,
    IOpenLibraryApi openLibraryApi,
    IImageService imageService,
    IDataRepository dataRepository)
    : TableBaseService<MetadataBookModel>(dataRepository), IBookMetadataService
{
    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var googleBooksResults = await googleBooksApi.GetBookMetadata
        (
            title: title
        );

        var googleBooksData = googleBooksResults?.Items?.FirstOrDefault()?.VolumeInfo;

           var searchResult = await openLibraryApi.SearchBook(title, true, language ?? "en");

        var openLibraryData = searchResult?.Docs[0]; // Take the first result as the best match

        var openLibraryBookDetails = await openLibraryApi.GetBookDetails(openLibraryData?.Key);
        var description = openLibraryBookDetails?.Description ?? openLibraryBookDetails?.Description?.ToString();

        var works = await openLibraryApi.GetWorks(openLibraryData?.Key);

        string? coverUrl = openLibraryApi.GetCover(false, openLibraryData, works?.Entries) ?? googleBooksData?.ImageLinks?.Thumbnail;
        var thumbnailBlurHash = await WriteImageAndReturnBlurHash(coverUrl, "cover", category: "Book", metadataId.ToString());

        var book = new MetadataBookModel()
        {
            Id = Guid.NewGuid(),
            Author = openLibraryData?.AuthorName?.FirstOrDefault() ?? googleBooksData?.Authors?.FirstOrDefault(),
            Publisher = googleBooksData?.Publisher,
            PublishedDate = googleBooksData?.PublishedDate,
            Description = description ?? googleBooksData?.Description,
            PageCount = googleBooksData?.PageCount,
            Language = googleBooksData?.Language,
            Thumbnail = coverUrl != null ? $"{Globals.Domain}/images/Book/{metadataId}/cover" : null,
            ThumbnailBlurHash = thumbnailBlurHash
        };

        await UpdateOrInsert(book);
        
        var metadata = new MetadataModel()
        {
            Title = googleBooksData?.Title,
            BookMetadataId = book.Id,
        };

        return metadata;
    }
    
    private async Task<string?> WriteImageAndReturnBlurHash(string? url, string fileName, string category, string id)
    {
        if (url == null)
            return null;

        var (bytes, imageType) = await openLibraryApi.GetBytesFromUrlAsync(url);

        imageType = imageType?.Split("/").LastOrDefault();
        
        await imageService.WriteImage(bytes, url, fileName, category, id, imageType);

        return imageService.CreateBlurHash(bytes);
    }
}
