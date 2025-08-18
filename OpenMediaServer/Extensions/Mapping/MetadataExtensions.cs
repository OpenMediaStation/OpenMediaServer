using OpenMediaServer.DTOs.Endpoints.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Extensions.Mapping;

public static class MetadataExtensions
{
    public static MetadataDto ToDto(this MetadataModel metadataModel, Guid inventoryItemId, MetadataMovieModel movie,
        MetadataShowModel show, MetadataEpisodeModel episode, MetadataSeasonModel season,
        MetadataAudiobookModel audiobook, MetadataBookModel book, IEnumerable<MetadataChapter> chapters)
    {
        var result = new MetadataDto()
        {
            Id = metadataModel.Id,
            Title = metadataModel.Title,
            Category = metadataModel.Category,
            ParentId = inventoryItemId,
            Movie = movie.ToDto(),
            Show = show.ToDto(),
            Episode = episode.ToDto(),
            Season = season.ToDto(),
            Audiobook = audiobook.ToDto(chapters),
            Book = book.ToDto()
        };

        return result;
    }

    public static MetadataMovieDto ToDto(this MetadataMovieModel model)
    {
        var result = new MetadataMovieDto()
        {
            Year = model.Year,
            Rated = model.Rated,
            Released = model.Released,
            Runtime = model.Runtime,
            Genre = model.Genre,
            Director = model.Director,
            Writer = model.Writer,
            Actors = model.Actors,
            Plot = model.Plot,
            Language = model.Language,
            Country = model.Country,
            Awards = model.Awards,
            Poster = model.Poster,
            PosterBlurHash = model.PosterBlurHash,
            Backdrop = model.Backdrop,
            BackdropBlurHash = model.BackdropBlurHash,
            Logo = model.Logo,
            LogoBlurHash = model.LogoBlurHash,
            Ratings = [],
            Metascore = model.Metascore,
            ImdbRating = model.ImdbRating,
            ImdbVotes = model.ImdbVotes,
            ImdbID = model.ImdbID,
            Type = model.Type,
            DVD = model.DVD,
            BoxOffice = model.BoxOffice,
            Production = model.Production,
            Website = model.Website,
        };

        return result;
    }

    public static MetadataShowDto ToDto(this MetadataShowModel model)
    {
        var result = new MetadataShowDto()
        {
            Year = model.Year,
            Rated = model.Rated,
            Released = model.Released,
            Runtime = model.Runtime,
            Genre = model.Genre,
            Director = model.Director,
            Writer = model.Writer,
            Actors = model.Actors,
            Plot = model.Plot,
            Language = model.Language,
            Country = model.Country,
            Awards = model.Awards,
            Poster = model.Poster,
            PosterBlurHash = model.PosterBlurHash,
            Backdrop = model.Backdrop,
            BackdropBlurHash = model.BackdropBlurHash,
            Logo = model.Logo,
            LogoBlurHash = model.LogoBlurHash,
            Ratings = [],
            Metascore = model.Metascore,
            ImdbRating = model.ImdbRating,
            ImdbVotes = model.ImdbVotes,
            ImdbID = model.ImdbID,
            Type = model.Type,
            DVD = model.DVD,
            BoxOffice = model.BoxOffice,
            Production = model.Production,
            Website = model.Website,
        };

        return result;
    }

    public static MetadataSeasonDto ToDto(this MetadataSeasonModel model)
    {
        var result = new MetadataSeasonDto()
        {
            Poster = model.Poster,
            PosterBlurHash = model.PosterBlurHash,
            AirDate = model.AirDate,
            EpisodeCount = model.EpisodeCount,
            Overview = model.Overview,
        };

        return result;
    }

    public static MetadataEpisodeDto ToDto(this MetadataEpisodeModel model)
    {
        var result = new MetadataEpisodeDto()
        {
            Year = model.Year,
            Rated = model.Rated,
            Released = model.Released,
            Runtime = model.Runtime,
            Genre = model.Genre,
            Director = model.Director,
            Writer = model.Writer,
            Actors = model.Actors,
            Plot = model.Plot,
            Language = model.Language,
            Country = model.Country,
            Awards = model.Awards,
            Backdrop = model.Backdrop,
            BackdropBlurHash = model.BackdropBlurHash,
            Ratings = [],
            Metascore = model.Metascore,
            ImdbRating = model.ImdbRating,
            ImdbVotes = model.ImdbVotes,
            ImdbID = model.ImdbID,
            Type = model.Type,
            DVD = model.DVD,
            BoxOffice = model.BoxOffice,
            Production = model.Production,
            Website = model.Website,
        };

        return result;
    }

    public static MetadataBookDto ToDto(this MetadataBookModel model)
    {
        var result = new MetadataBookDto()
        {
            Authors = [model.Author],
            Publisher = model.Publisher,
            PublishedDate = model.PublishedDate,
            Description = model.Description,
            PageCount = model.PageCount,
            Language = model.Language,
            Thumbnail = model.Thumbnail,
            ThumbnailBlurHash = model.ThumbnailBlurHash
        };

        return result;
    }

    public static MetadataAudiobookDto ToDto(this MetadataAudiobookModel model, IEnumerable<MetadataChapter> chapters)
    {
        var result = new MetadataAudiobookDto()
        {
            Authors = [model.Author],
            Publisher = model.Publisher,
            PublishedDate = model.PublishedDate,
            Description = model.Description,
            Language = model.Language,
            Thumbnail = model.Thumbnail,
            ThumbnailBlurHash = model.ThumbnailBlurHash,
            Chapters = chapters.Select(i => i.ToDto())
        };

        return result;
    }

    public static MetadataChapterDto ToDto(this MetadataChapter chapter)
    {
        var result = new MetadataChapterDto()
        {
            Title = chapter.Title,
            StartTimeInSeconds = chapter.StartTimeInSeconds,
            EndTimeInSeconds = chapter.EndTimeInSeconds,
        };

        return result;
    }

}