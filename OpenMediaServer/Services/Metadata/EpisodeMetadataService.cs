using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;
using TMDbLib.Objects.TvShows;

namespace OpenMediaServer.Services.Metadata;

public class EpisodeMetadataService(
    IOmdbAPI omdbApi,
    ITMDbAPI tMDbApi,
    IImageService imageService,    ILogger<EpisodeMetadataService> logger,
    IDataRepository dataRepository) : TableBaseService<MetadataEpisodeModel>(dataRepository), IEpisodeMetadataService
{
    public async Task<MetadataModel> GetEpisodeMetadata(string? year, string title, string? language, Guid metadataId,
        int? season, int? episode)
    {
        var omdbData = await omdbApi.GetMetadata
        (
            name: title,
            apiKey: Globals.OmdbApiKey,
            year: year,
            season: season,
            episode: episode
        );

        var showData = await tMDbApi.GetShow
        (
            name: title,
            apiKey: Globals.TmdbApiKey,
            year: year
        );

        TvEpisode? episodeInfo = null;

        if (showData != null && season != null && episode != null)
        {
            episodeInfo = await tMDbApi.GetEpisode(showData.Id, (int)season, (int)episode, Globals.TmdbApiKey);
        }

        var backdropBlurHash =
            await WriteImageAndReturnBlurHash(episodeInfo?.StillPath, "backdrop", "Episode", metadataId.ToString());

        var episodeMetadata = new MetadataEpisodeModel()
        {
            Id = Guid.NewGuid(),
            Year = omdbData?.Year,
            Rated = omdbData?.Rated,
            Released = omdbData?.Released,
            Runtime = omdbData?.Runtime,
            Genre = omdbData?.Genre,
            Director = omdbData?.Director,
            Writer = omdbData?.Writer,
            Actors = omdbData?.Actors,
            Plot = episodeInfo?.Overview ?? omdbData?.Plot,
            Language = omdbData?.Language,
            Country = omdbData?.Country,
            Awards = omdbData?.Awards,
            Backdrop = episodeInfo?.StillPath != null
                ? $"{Globals.Domain}/images/Episode/{metadataId}/backdrop"
                : omdbData?.Poster,
            BackdropBlurHash = backdropBlurHash,
            Metascore = omdbData?.Metascore,
            ImdbRating = omdbData?.ImdbRating,
            ImdbVotes = omdbData?.ImdbVotes,
            ImdbID = omdbData?.ImdbID,
            Type = omdbData?.Type,
            DVD = omdbData?.DVD,
            BoxOffice = omdbData?.BoxOffice,
            Production = omdbData?.Production,
            Website = omdbData?.Website,
        };

        await UpdateOrInsert(episodeMetadata);

        var metadata = new MetadataModel()
        {
            Title = episodeInfo?.Name ?? omdbData?.Title,
            EpisodeMetadataId = episodeMetadata.Id,
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