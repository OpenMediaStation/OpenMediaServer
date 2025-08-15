using ATL;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class AudiobookMetadataService(
    IGoogleBooksApi googleBooksApi,
    IOpenLibraryApi openLibraryApi,
    IImageService imageService,
    IDataRepository dataRepository)
    : TableBaseService<MetadataAudiobookModel>(dataRepository), IAudioBookMetadataService
{
    public async Task<MetadataModel> GenerateMetadata(string? year, string title, string? language, Guid metadataId, string? filePath)
    {
        MetadataAudiobookModel? fileExtract = null;
        string? extractedTitle = null;

        if (filePath != null)
        {
            (fileExtract, extractedTitle) = await ExtractMetadataFromAudioFile(filePath);
        }

        var googleBooksResult = await googleBooksApi.GetBookMetadata(title);
        var searchResult = await openLibraryApi.SearchBook(title, true, language ?? "en");
        var openLibraryData = searchResult?.Docs.FirstOrDefault();
        var openLibraryBookDetails = await openLibraryApi.GetBookDetails(openLibraryData?.Key);
        var description = openLibraryBookDetails?.Description?.ToString();
        var works = await openLibraryApi.GetWorks(openLibraryData?.Key);
        var googleBooksData = googleBooksResult?.Items?.FirstOrDefault()?.VolumeInfo;

        var audiobook = new MetadataAudiobookModel()
        {
            Id = Guid.NewGuid(),
            Authors = fileExtract?.Authors ?? openLibraryData?.AuthorName?.FirstOrDefault() ?? googleBooksData?.Authors.FirstOrDefault(),
            Publisher = fileExtract?.Publisher ?? googleBooksData?.Publisher,
            PublishedDate = fileExtract?.PublishedDate ?? googleBooksData?.PublishedDate,
            Description = fileExtract?.Description ?? description ?? googleBooksData?.Description,
            Language = fileExtract?.Language ?? googleBooksData?.Language,
            Thumbnail = fileExtract?.Thumbnail,
            ThumbnailBlurHash = fileExtract?.ThumbnailBlurHash,
        };
        
        if (audiobook.Thumbnail == null)
        {
            string? coverUrl = openLibraryApi.GetCover(true, openLibraryData, works?.Entries) ?? googleBooksData?.ImageLinks?.Thumbnail;
            (coverUrl, var blurHash) = await WriteImageAndReturnPathAndBlurHash(coverUrl, metadataId.ToString());

            audiobook.Thumbnail = coverUrl;
            audiobook.ThumbnailBlurHash = blurHash;
        }
        
        await UpdateOrInsert(audiobook);
        
        var metadata = new MetadataModel()
        {
            Title = extractedTitle ?? openLibraryData?.Title ?? googleBooksData?.Title,
            AudiobookMetadataId = audiobook.Id,
        };

        return metadata;
    }

    public async Task<(MetadataAudiobookModel, string)> ExtractMetadataFromAudioFile(string filePath)
    {
        var file = TagLib.File.Create(filePath);
        var (thumbnailPath, thumbnailBlurHash) = await ExtractCoverArt(file);
        return (new MetadataAudiobookModel()
        {
            Authors = file.Tag.Performers?.FirstOrDefault(),
            Publisher = file.Tag.Publisher,
            PublishedDate = file.Tag.Year.ToString(),
            Description = file.Tag.Comment,
            // Language = file.Tag.Languages?.FirstOrDefault(),
            Thumbnail = thumbnailPath,
            ThumbnailBlurHash = thumbnailBlurHash
        }, file.Tag.Title);
    }

    private async Task<(string? Path, string? BlurHash)> ExtractCoverArt(TagLib.File file)
    {
        if (file.Tag.Pictures.Length > 0)
        {
            var picture = file.Tag.Pictures[0];
            var coverData = picture.Data.Data;
            var imageType = picture.MimeType.Split('/').LastOrDefault();
            var metadataId = Guid.NewGuid().ToString();
            
            var coverPath =  await imageService.WriteImage(coverData, "", "cover", "Audiobook", metadataId, imageType);
            var blurHash = imageService.CreateBlurHash(coverData);
            return (coverPath, blurHash);
        }
        return (null,null);
    }

    private async Task<(string? Path, string? BlurHash)> WriteImageAndReturnPathAndBlurHash(string? coverUrl, string metadataId)
    {
        if (coverUrl == null)
            return (null,null);

        var (bytes, imageType) = await openLibraryApi.GetBytesFromUrlAsync(coverUrl);

        imageType = imageType?.Split("/").LastOrDefault();

        var imagePath = await imageService.WriteImage(bytes, coverUrl, "cover", "Audiobook", metadataId, imageType: imageType);
        var blurHash = imageService.CreateBlurHash(bytes);
        return (imagePath, blurHash);
    }
}
