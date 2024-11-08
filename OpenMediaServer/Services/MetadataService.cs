using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Metadata;
using TMDbLib.Objects.General;
using TMDbLib.Objects.TvShows;

namespace OpenMediaServer.Services;

public class MetadataService : IMetadataService
{
    private readonly ILogger<MetadataService> _logger;
    private readonly IOmdbAPI _omdbAPI;
    private readonly IConfiguration _configuration;
    private readonly IFileSystemRepository _storageRepository;
    private readonly IGoogleBooksApi _googleBooksApi;
    private readonly ITMDbAPI _tMDbAPI;
    private readonly IImageService _imageService;

    public MetadataService(ILogger<MetadataService> logger, IOmdbAPI omdbAPI, IConfiguration configuration, IFileSystemRepository storageRepository, IGoogleBooksApi googleBooksApi, ITMDbAPI tMDbAPI, IImageService imageService)
    {
        _logger = logger;
        _omdbAPI = omdbAPI;
        _configuration = configuration;
        _storageRepository = storageRepository;
        _googleBooksApi = googleBooksApi;
        _tMDbAPI = tMDbAPI;
        _imageService = imageService;
    }

    public async Task<MetadataModel?> CreateNewMetadata(string category, Guid parentId, string title, string? year = null, int? season = null, int? episode = null, string? language = "en")
    {
        var metadatas = await ListMetadata(category);

        MetadataModel? metadata = new();
        var metadataId = Guid.NewGuid();

        switch (category)
        {
            case "Movie":
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

                    await WriteImage(tmdbData?.BackdropPath, "backdrop", "Movie", metadataId.ToString());
                    await WriteImage(logoPath, "logo", "Movie", metadataId.ToString());
                    await WriteImage(posterPath, "poster", "Movie", metadataId.ToString());

                    metadata = new MetadataModel()
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
                            Plot = omdbData?.Plot ?? tmdbData?.Overview,
                            Language = omdbData?.Language,
                            Country = omdbData?.Country,
                            Awards = omdbData?.Awards,
                            Poster = posterPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/poster" : omdbData?.Poster,
                            Backdrop = tmdbData?.BackdropPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/backdrop" : null,
                            Logo = logoPath != null ? $"{Globals.Domain}/images/Movie/{metadataId}/logo" : null,
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

                    break;
                }
            case "Show":
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

                    await WriteImage(tmdbData?.BackdropPath, "backdrop", "Show", metadataId.ToString());
                    await WriteImage(logoPath, "logo", "Show", metadataId.ToString());
                    await WriteImage(posterPath, "poster", "Show", metadataId.ToString());

                    metadata = new MetadataModel()
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
                            Plot = omdbData?.Plot,
                            Language = omdbData?.Language,
                            Country = omdbData?.Country,
                            Awards = omdbData?.Awards,
                            Poster = posterPath != null ? $"{Globals.Domain}/images/Show/{metadataId}/poster" : omdbData?.Poster,
                            Backdrop = tmdbData?.BackdropPath != null ? $"{Globals.Domain}/images/Show/{metadataId}/backdrop" : null,
                            Logo = logoPath != null ? $"{Globals.Domain}/images/Show/{metadataId}/logo" : null,
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

                    break;
                }

            case "Season":
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

                    await WriteImage(seasonInfo?.PosterPath, "poster", "Season", metadataId.ToString());

                    metadata = new MetadataModel()
                    {
                        Title = tmdbData?.Name,
                        Season = new()
                        {
                            Poster = seasonInfo?.PosterPath != null ? $"{Globals.Domain}/images/Season/{metadataId}/poster" : null,
                            AirDate = seasonInfo?.AirDate,
                            EpisodeCount = seasonInfo?.Episodes.Count,
                            Overview = seasonInfo?.Overview,
                        }
                    };

                    break;
                }

            case "Episode":
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

                    await WriteImage(episodeInfo?.StillPath, "backdrop", "Episode", metadataId.ToString());

                    metadata = new MetadataModel()
                    {
                        Title = omdbData?.Title,
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
                            Plot = omdbData?.Plot,
                            Language = omdbData?.Language,
                            Country = omdbData?.Country,
                            Awards = omdbData?.Awards,
                            Backdrop = episodeInfo?.StillPath != null ? $"{Globals.Domain}/images/Episode/{metadataId}/backdrop" : omdbData?.Poster,
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

                    break;
                }

            case "Book":
                {
                    var result = await _googleBooksApi.GetBookMetadata
                    (
                        title: title
                    );

                    var data = result?.Items?.FirstOrDefault()?.VolumeInfo;

                    metadata = new MetadataModel()
                    {
                        Title = data?.Title,
                        Book = new()
                        {
                            Authors = data?.Authors,
                            Publisher = data?.Publisher,
                            PublishedDate = data?.PublishedDate,
                            Description = data?.Description,
                            PageCount = data?.PageCount,
                            Language = data?.Language,
                            Thumbnail = data?.ImageLinks?.Thumbnail
                        }
                    };

                    break;
                }

            default:
                {
                    _logger.LogWarning("Cannot create metadata for type {Type}", category);

                    return null;
                }
        }
        metadata.Id = metadataId;
        metadata.Category = category;
        metadata.ParentId = parentId;

        metadatas = metadatas.Append(metadata);

        await _storageRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "metadata", category) + ".json", metadatas);

        return metadata;
    }

    public async Task<IEnumerable<MetadataModel>> ListMetadata(string category)
    {
        var metadatas = await _storageRepository.ReadObject<IEnumerable<MetadataModel>>(Path.Combine(Globals.ConfigFolder, "metadata", category) + ".json");

        metadatas ??= [];

        return metadatas;
    }

    public async Task<MetadataModel?> GetMetadata(string category, Guid id)
    {
        var metadatas = await _storageRepository.ReadObject<IEnumerable<MetadataModel>>(Path.Combine(Globals.ConfigFolder, "metadata", category) + ".json");

        var metadata = metadatas?.FirstOrDefault(x => x.Id == id);

        return metadata;
    }

    public async Task<bool> UpdateOrAddMetadata(MetadataModel metadataModel)
    {
        if (metadataModel.Category == null)
        {
            return false;
        }

        var metadatas = (await ListMetadata(metadataModel.Category)).ToList();

        var existingMetadata = metadatas.FirstOrDefault(m => m.Id == metadataModel.Id);

        if (existingMetadata != null)
        {
            var index = metadatas.IndexOf(existingMetadata);
            metadatas[index] = metadataModel;
        }
        else
        {
            metadatas.Add(metadataModel);
        }

        await _storageRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "metadata", metadataModel.Category) + ".json", metadatas);

        return true;
    }

    private async Task WriteImage(string? url, string fileName, string category, string id)
    {
        if (url == null)
            return;

        var bytes = await _tMDbAPI.GetImageFromId(url, Globals.TmdbApiKey);

        await _imageService.WriteImage(bytes, url, fileName, category, id);
    }
}
