using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Metadata;
using TMDbLib.Objects.General;
using TMDbLib.Objects.TvShows;

namespace OpenMediaServer.Services.Metadata;

public class ShowMetadataService : IShowMetadataService
{
    private readonly IOmdbAPI _omdbAPI;
    private readonly ITMDbAPI _tMDbAPI;
    private readonly IImageService _imageService;

    public ShowMetadataService(IOmdbAPI omdbAPI, ITMDbAPI tMDbAPI, IImageService imageService)
    {
        _omdbAPI = omdbAPI;
        _tMDbAPI = tMDbAPI;
        _imageService = imageService;
    }

    public async Task<MetadataModel> GetEpisodeMetadata(string? year, string title, string? language, Guid metadataId, int? season, int? episode)
    {
        var omdbData = await _omdbAPI.GetMetadata
        (
            name: title,
            apiKey: Globals.OmdbApiKey,
            year: year,
            season: season,
            episode: episode
        );

        var showData = await _tMDbAPI.GetShow
        (
            name: title,
            apiKey: Globals.TmdbApiKey,
            year: year
        );

        TvEpisode? episodeInfo = null;

        if (showData != null && season != null && episode != null)
        {
            episodeInfo = await _tMDbAPI.GetEpisode(showData.Id, (int)season, (int)episode, Globals.TmdbApiKey);
        }

        var backdropBlurHash = await WriteImageAndReturnBlurHash(episodeInfo?.StillPath, "backdrop", "Episode", metadataId.ToString());

        var metadata = new MetadataModel()
        {
            Title = episodeInfo?.Name ?? omdbData?.Title,
            Episode = new()
            {
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
                Backdrop = episodeInfo?.StillPath != null ? $"{Globals.Domain}/images/Episode/{metadataId}/backdrop" : omdbData?.Poster,
                BackdropBlurHash = backdropBlurHash,
                Ratings = omdbData?.Ratings?.ConvertAll(rating => new Rating
                {
                    Source = rating.Source,
                    Value = rating.Value
                }),
                Metascore = omdbData?.Metascore,
                ImdbRating = omdbData?.ImdbRating,
                ImdbVotes = omdbData?.ImdbVotes,
                ImdbID = omdbData?.ImdbID,
                Type = omdbData?.Type,
                DVD = omdbData?.DVD,
                BoxOffice = omdbData?.BoxOffice,
                Production = omdbData?.Production,
                Website = omdbData?.Website,
            }
        };

        return metadata;
    }

    public async Task<MetadataModel> GetSeasonMetadata(string? year, string title, string? language, Guid metadataId, int? season)
    {
        var tmdbData = await _tMDbAPI.GetShow
                   (
                       name: title,
                       apiKey: Globals.TmdbApiKey,
                       year: year
                   );

        TvSeason? seasonInfo = null;

        if (tmdbData != null && season != null)
        {
            seasonInfo = await _tMDbAPI.GetSeason(tmdbData.Id, (int)season, Globals.TmdbApiKey);
        }

        var posterBlurHash = await WriteImageAndReturnBlurHash(seasonInfo?.PosterPath, "poster", "Season", metadataId.ToString());

        var metadata = new MetadataModel()
        {
            Title = seasonInfo?.Name,
            Season = new()
            {
                Poster = seasonInfo?.PosterPath != null ? $"{Globals.Domain}/images/Season/{metadataId}/poster" : null,
                PosterBlurHash = posterBlurHash,
                AirDate = seasonInfo?.AirDate,
                EpisodeCount = seasonInfo?.Episodes.Count,
                Overview = seasonInfo?.Overview,
            }
        };

        return metadata;
    }

    public async Task<MetadataModel> GetShowMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var omdbData = await _omdbAPI.GetMetadata
        (
            name: title,
            apiKey: Globals.OmdbApiKey,
            year: year
        );

        var tmdbData = await _tMDbAPI.GetShow
        (
            name: title,
            apiKey: Globals.TmdbApiKey,
            year: year
        );

        ImagesWithId? tmdbImages = null;

        if (tmdbData?.Id != null)
        {
            tmdbImages = await _tMDbAPI.GetShowImages(tmdbData.Id, apiKey: Globals.TmdbApiKey);
        }

        var logoPath = tmdbImages?.Logos.Where(i => i.Iso_639_1 == language).FirstOrDefault()?.FilePath;
        var posterPath = tmdbImages?.Posters.Where(i => i.Iso_639_1 == language).FirstOrDefault()?.FilePath;

        var backdropBlurHash = await WriteImageAndReturnBlurHash(tmdbData?.BackdropPath, "backdrop", "Show", metadataId.ToString());
        var logoBlurHash = await WriteImageAndReturnBlurHash(logoPath, "logo", "Show", metadataId.ToString());
        var posterBlurHash = await WriteImageAndReturnBlurHash(posterPath, "poster", "Show", metadataId.ToString());

        var metadata = new MetadataModel()
        {
            Title = omdbData?.Title,
            Show = new()
            {
                Year = omdbData?.Year,
                Rated = omdbData?.Rated,
                Released = omdbData?.Released,
                Runtime = omdbData?.Runtime,
                Genre = omdbData?.Genre,
                Director = omdbData?.Director,
                Writer = omdbData?.Writer,
                Actors = omdbData?.Actors,
                Plot = tmdbData?.Overview ?? omdbData?.Plot,
                Language = omdbData?.Language,
                Country = omdbData?.Country,
                Awards = omdbData?.Awards,
                Poster = posterPath != null ? $"{Globals.Domain}/images/Show/{metadataId}/poster" : omdbData?.Poster,
                PosterBlurHash = posterBlurHash,
                Backdrop = tmdbData?.BackdropPath != null ? $"{Globals.Domain}/images/Show/{metadataId}/backdrop" : null,
                BackdropBlurHash = backdropBlurHash,
                Logo = logoPath != null ? $"{Globals.Domain}/images/Show/{metadataId}/logo" : null,
                LogoBlurHash = logoBlurHash,
                Ratings = omdbData?.Ratings?.ConvertAll(rating => new Rating
                {
                    Source = rating.Source,
                    Value = rating.Value
                }),
                Metascore = omdbData?.Metascore,
                ImdbRating = omdbData?.ImdbRating,
                ImdbVotes = omdbData?.ImdbVotes,
                ImdbID = omdbData?.ImdbID,
                Type = omdbData?.Type,
                DVD = omdbData?.DVD,
                BoxOffice = omdbData?.BoxOffice,
                Production = omdbData?.Production,
                Website = omdbData?.Website,
            }
        };

        return metadata;
    }

    private async Task<string?> WriteImageAndReturnBlurHash(string? url, string fileName, string category, string id)
    {
        if (url == null)
            return null;

        var bytes = await _tMDbAPI.GetImageFromId(url, Globals.TmdbApiKey);

        await _imageService.WriteImage(bytes, url, fileName, category, id);

        return _imageService.CreateBlurHash(bytes);
    }
}
