using ATL;
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

    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId, string? filePath)
    {
        MetadataModel metadata = new();

        if (filePath != null)
        {
            metadata = await ExtractMetadataFromAudioFile(filePath);
        }

        var googleBooksResult = await _googleBooksApi.GetBookMetadata(title);
        var searchResult = await _openLibraryApi.SearchBook(title, true, language ?? "en");
        var openLibraryData = searchResult?.Docs.FirstOrDefault();
        var openLibraryBookDetails = await _openLibraryApi.GetBookDetails(openLibraryData?.Key);
        var description = openLibraryBookDetails?.Description?.ToString();
        var works = await _openLibraryApi.GetWorks(openLibraryData?.Key);
        var googleBooksData = googleBooksResult?.Items?.FirstOrDefault()?.VolumeInfo;

        metadata = new MetadataModel()
        {
            Title = metadata.Title ?? openLibraryData?.Title ?? googleBooksData?.Title,
            Audiobook = new()
            {
                Authors = metadata.Audiobook?.Authors ?? openLibraryData?.AuthorName ?? googleBooksData?.Authors,
                Publisher = metadata.Audiobook?.Publisher ?? googleBooksData?.Publisher,
                PublishedDate = metadata.Audiobook?.PublishedDate ?? googleBooksData?.PublishedDate,
                Description = metadata.Audiobook?.Description ?? description ?? googleBooksData?.Description,
                Language = metadata.Audiobook?.Language ?? googleBooksData?.Language,
                Thumbnail = metadata.Audiobook?.Thumbnail
            }
        };

        if (metadata.Audiobook.Thumbnail == null)
        {
            string? coverUrl = _openLibraryApi.GetCover(true, openLibraryData, works?.Entries) ?? googleBooksData?.ImageLinks?.Thumbnail;
            coverUrl = await SaveImage(coverUrl, metadataId.ToString());

            metadata.Audiobook.Thumbnail = coverUrl;
        }

        metadata.Audiobook.Chapters = ExtractChapters(filePath);

        return metadata;
    }

    public List<MetadataAudiobookChapter> ExtractChapters(string? filePath)
    {
        if (filePath == null)
        {
            return [];
        }

        Track track = new Track(filePath);

        List<MetadataAudiobookChapter> chapters = [];

        foreach (var item in track.Chapters)
        {
            chapters.Add(new MetadataAudiobookChapter()
            {
                Title = item.Title,
                StartTimeInSeconds = item.StartTime / 1000,
                EndTimeInSeconds = item.EndTime / 1000,
            });
        }

        return chapters;
    }

    public async Task<MetadataModel> ExtractMetadataFromAudioFile(string filePath)
    {
        var file = TagLib.File.Create(filePath);
        return new MetadataModel()
        {
            Title = file.Tag.Title,
            Audiobook = new()
            {
                Authors = file.Tag.Performers?.ToList(),
                Publisher = file.Tag.Publisher,
                PublishedDate = file.Tag.Year.ToString(),
                Description = file.Tag.Comment,
                // Language = file.Tag.Languages?.FirstOrDefault(),
                Thumbnail = await ExtractCoverArt(file)
            }
        };
    }

    private async Task<string?> ExtractCoverArt(TagLib.File file)
    {
        if (file.Tag.Pictures.Length > 0)
        {
            var picture = file.Tag.Pictures[0];
            var coverData = picture.Data.Data;
            var imageType = picture.MimeType.Split('/').LastOrDefault();
            var metadataId = Guid.NewGuid().ToString();
            return await _imageService.WriteImage(coverData, "", "cover", "Audiobook", metadataId, imageType);
        }
        return null;
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
