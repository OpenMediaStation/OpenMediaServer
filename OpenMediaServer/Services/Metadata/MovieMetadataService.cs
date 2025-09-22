using OpenMediaServer.DTOs;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Metadata;
using TMDbLib.Objects.General;

namespace OpenMediaServer.Services.Metadata;

public class MovieMetadataService(IOmdbAPI omdbApi, ITMDbAPI tMDbApi, IImageService imageService, IDataRepository dataRepository) : TableBaseService<MetadataMovieModel>(dataRepository), IMovieMetadataService
{
    public async Task<MetadataModel> GetMetadata(string? year, string title, string? language, Guid metadataId)
    {
        var omdbData = await omdbApi.GetMetadata
        (
            name: title,
            apiKey: Globals.OmdbApiKey,
            year: year
        );

        var tmdbData = await tMDbApi.GetMovie
        (
            name: title,
            apiKey: Globals.TmdbApiKey,
            year: year
        );

        ImagesWithId? tmdbImages = null;

        if (tmdbData?.Id != null)
        {
            tmdbImages = await tMDbApi.GetMovieImages(tmdbData.Id, apiKey: Globals.TmdbApiKey);
        }

        var tmdbLogosSorted = tmdbImages?.Logos?.OrderBy(i => i.VoteAverage)?.ToList();
        var tmdbPostersSorted = tmdbImages?.Posters?.OrderBy(i => i.VoteAverage)?.ToList();
        
        var logoPath = tmdbLogosSorted?.FirstOrDefault(i => i.Iso_639_1 == language)?.FilePath ??
                       tmdbLogosSorted?.FirstOrDefault()?.FilePath;
        var posterPath = tmdbPostersSorted?.FirstOrDefault(i => i.Iso_639_1 == language)?.FilePath ?? 
                         tmdbPostersSorted?.FirstOrDefault()?.FilePath ?? omdbData?.Poster;

        var backdropBlurHash = await WriteImageAndReturnBlurHash(tmdbData?.BackdropPath, "backdrop", "Movie", metadataId.ToString());
        var logoBlurHash = await WriteImageAndReturnBlurHash(logoPath, "logo", "Movie", metadataId.ToString());
        var posterBlurHash = await WriteImageAndReturnBlurHash(posterPath, "poster", "Movie", metadataId.ToString());

        var movie = new  MetadataMovieModel()
        {
            Id = Guid.NewGuid(),
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
            Poster = posterPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/poster" : null,
            PosterBlurHash = posterBlurHash,
            Backdrop = tmdbData?.BackdropPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/backdrop" : null,
            BackdropBlurHash = backdropBlurHash,
            Logo = logoPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/logo" : null,
            LogoBlurHash = logoBlurHash,
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
        
        await UpdateOrInsert(movie);
        
        var metadata = new MetadataModel()
        {
            Title = omdbData?.Title ?? tmdbData?.Title,
            MovieMetadataId = movie.Id,
        };

        return metadata;
    }

    private async Task<string?> WriteImageAndReturnBlurHash(string? url, string fileName, string category, string id)
    {
        if (url == null)
            return null;

        var bytes = await tMDbApi.GetImageFromId(url, Globals.TmdbApiKey);

        await imageService.WriteImage(bytes, url, fileName, category, id);

        return imageService.CreateBlurHash(bytes);
    }
}
