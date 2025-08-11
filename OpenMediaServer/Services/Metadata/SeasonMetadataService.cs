using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;
using TMDbLib.Objects.TvShows;

namespace OpenMediaServer.Services.Metadata;

public class SeasonMetadataService(
    ITMDbAPI tMDbApi,
    IImageService imageService,
    ILogger<SeasonMetadataService> logger,
    IDataRepository dataRepository) : TableBaseService<MetadataSeasonModel>(dataRepository), ISeasonMetadataService
{
    public async Task<MetadataModel> GetSeasonMetadata(string? year, string title, string? language, Guid metadataId,
        int? season)
    {
        var tmdbData = await tMDbApi.GetShow
        (
            name: title,
            apiKey: Globals.TmdbApiKey,
            year: year
        );

        TvSeason? seasonInfo = null;

        if (tmdbData != null && season != null)
        {
            seasonInfo = await tMDbApi.GetSeason(tmdbData.Id, (int)season, Globals.TmdbApiKey);
        }

        var posterBlurHash =
            await WriteImageAndReturnBlurHash(seasonInfo?.PosterPath, "poster", "Season", metadataId.ToString());

        var seasonMetadata = new MetadataSeasonModel()
        {
            Poster = seasonInfo?.PosterPath != null ? $"{Globals.Domain}/images/Season/{metadataId}/poster" : null,
            PosterBlurHash = posterBlurHash,
            AirDate = seasonInfo?.AirDate,
            EpisodeCount = seasonInfo?.Episodes.Count,
            Overview = seasonInfo?.Overview,
        };

        await UpdateOrInsert(seasonMetadata);

        var metadata = new MetadataModel()
        {
            Title = seasonInfo?.Name,
            SeasonMetadataId = seasonMetadata.Id,
        };

        return metadata;
    }

    private async Task<string?> WriteImageAndReturnBlurHash(string? url, string fileName, string category, string id)
    {
        if (url == null)
            return null;

        byte[]? bytes = null;

        if (url.StartsWith("http"))
        {
            try
            {
                bytes = await new HttpClient().GetByteArrayAsync(url);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError($"Unable to get image from url: {url}, message: {ex.Message}");
            }
        }
        else
        {
            bytes = await tMDbApi.GetImageFromId(url, Globals.TmdbApiKey);
        }

        if (bytes == null)
        {
            return null;
        }

        await imageService.WriteImage(bytes, url, fileName, category, id);

        return imageService.CreateBlurHash(bytes);
    }
}