using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Metadata;
using TMDbLib.Objects.General;

namespace OpenMediaServer.Services.Metadata;

public class MovieMetadataService : IMovieMetadataService
{
    private readonly IOmdbAPI _omdbAPI;
    private readonly ITMDbAPI _tMDbAPI;
    private readonly IImageService _imageService;

    public MovieMetadataService(IOmdbAPI omdbAPI, ITMDbAPI tMDbAPI, IImageService imageService)
    {
        _omdbAPI = omdbAPI;
        _tMDbAPI = tMDbAPI;
        _imageService = imageService;
    }

    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var omdbData = await _omdbAPI.GetMetadata
        (
            name: title,
            apiKey: Globals.OmdbApiKey,
            year: year
        );

        var tmdbData = await _tMDbAPI.GetMovie
        (
            name: title,
            apiKey: Globals.TmdbApiKey,
            year: year
        );

        ImagesWithId? tmdbImages = null;

        if (tmdbData?.Id != null)
        {
            tmdbImages = await _tMDbAPI.GetMovieImages(tmdbData.Id, apiKey: Globals.TmdbApiKey);
        }

        var logoPath = tmdbImages?.Logos.Where(i => i.Iso_639_1 == language).FirstOrDefault()?.FilePath;
        var posterPath = tmdbImages?.Posters.Where(i => i.Iso_639_1 == language).FirstOrDefault()?.FilePath;

        var backdropBlurHash = await WriteImageAndReturnBlurHash(tmdbData?.BackdropPath, "backdrop", "Movie", metadataId.ToString());
        var logoBlurHash = await WriteImageAndReturnBlurHash(logoPath, "logo", "Movie", metadataId.ToString());
        var posterBlurHash = await WriteImageAndReturnBlurHash(posterPath, "poster", "Movie", metadataId.ToString());
        
        var metadata = new MetadataModel()
        {
            Title = omdbData?.Title ?? tmdbData?.Title,
            Movie = new()
            {
                Year = omdbData?.Year ?? tmdbData?.ReleaseDate.ToString(),
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
                Poster = posterPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/poster" : omdbData?.Poster,
                PosterBlurHash = posterBlurHash,
                Backdrop = tmdbData?.BackdropPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/backdrop" : null,
                BackdropBlurHash = backdropBlurHash,
                Logo = logoPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/logo" : null,
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
