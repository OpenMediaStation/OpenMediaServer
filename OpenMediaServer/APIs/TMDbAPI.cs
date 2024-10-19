using OpenMediaServer.Interfaces.APIs;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.TvShows;

namespace OpenMediaServer.APIs;

public class TMDbAPI : ITMDbAPI
{
    private readonly ILogger<TMDbAPI> _logger;
    private readonly HttpClient _httpClient;

    private const string TMDbBaseUrl = "https://api.themoviedb.org/3/";

    public TMDbAPI(ILogger<TMDbAPI> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Movie?> GetMovie(string name, string? apiKey, string? year = null)
    {
        TMDbClient client = new(apiKey);

        _ = int.TryParse(year, out var yearParsed);

        var result = await client.SearchMovieAsync
        (
            query: name,
            year: yearParsed
        );

        var search = result.Results.FirstOrDefault();

        if (search == null)
            return null;

        var movie = await client.GetMovieAsync(search.Id);

        return movie;
    }

    public async Task<TvShow?> GetShow(string name, string? apiKey, string? year = null)
    {
        TMDbClient client = new(apiKey);

        _ = int.TryParse(year, out var yearParsed);

        var result = await client.SearchTvShowAsync
        (
            query: name,
            firstAirDateYear: yearParsed
        );

        var search = result.Results.FirstOrDefault();

        if (search == null)
            return null;

        var show = await client.GetTvShowAsync(search.Id);

        return show;
    }

    public async Task<TvSeason?> GetSeason(int showId, int seasonNr, string? apiKey)
    {
        TMDbClient client = new(apiKey);

        var result = await client.GetTvSeasonAsync
        (
            tvShowId: showId,
            seasonNumber: seasonNr
        );

        return result;
    }

    public async Task<TvEpisode?> GetEpisode(int showId, int seasonNr, int episodeNr, string? apiKey)
    {
        TMDbClient client = new(apiKey);

        var result = await client.GetTvEpisodeAsync
        (
            tvShowId: showId,
            seasonNumber: seasonNr,
            episodeNumber: episodeNr
        );

        return result;
    }

    public async Task<Person?> GetPerson(string name, string? apiKey)
    {
        TMDbClient client = new(apiKey);

        var result = await client.SearchPersonAsync
        (
            query: name
        );

        var search = result.Results.FirstOrDefault();

        if (search == null)
            return null;

        var person = await client.GetPersonAsync(search.Id);

        return person;
    }

    public async Task<ImagesWithId?> GetMovieImages(int movieId, string? apiKey)
    {
        TMDbClient client = new TMDbClient(apiKey);

        var result = await client.GetMovieImagesAsync(movieId);

        return result;
    }

    public async Task<ImagesWithId?> GetShowImages(int showId, string? apiKey)
    {
        TMDbClient client = new TMDbClient(apiKey);

        var result = await client.GetTvShowImagesAsync(showId);

        return result;
    }

    public async Task<PosterImages?> GetSeasonImages(int showId, int seasonNr, string? apiKey)
    {
        TMDbClient client = new TMDbClient(apiKey);

        var result = await client.GetTvSeasonImagesAsync(showId, seasonNr);

        return result;
    }

    public async Task<StillImages?> GetEpisodeImages(int showId, int seasonNr, int episodeNr, string? apiKey)
    {
        TMDbClient client = new TMDbClient(apiKey);

        var result = await client.GetTvEpisodeImagesAsync(showId, seasonNr, episodeNr);

        return result;
    }

    public async Task<ProfileImages?> GetPersonImages(int personId, string? apiKey)
    {
        TMDbClient client = new TMDbClient(apiKey);

        var result = await client.GetPersonImagesAsync(personId);

        return result;
    }

    public async Task<byte[]?> GetImageFromId(string imagePath, string? apiKey)
    {
        TMDbClient client = new TMDbClient(apiKey);
        await client.GetConfigAsync();

        var result = await client.GetImageBytesAsync("original", imagePath);

        return result;
    }
}