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
        try
        {
            TMDbClient client = new(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

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
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get movie from tmdb, name:[{name}], year:[{year}] message: {ex.Message}]");
            return null;
        }
    }

    public async Task<TvShow?> GetShow(string name, string? apiKey, string? year = null)
    {
        try
        {
            TMDbClient client = new(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            _ = int.TryParse(year, out var yearParsed);

            var result = await client.SearchTvShowAsync
            (
                query: name,
                firstAirDateYear: yearParsed
            );

            var search = result.Results.FirstOrDefault();

            if (search == null)
                return null;

            var show = await client.GetTvShowAsync(search.Id, language: Globals.PreferredLanguage,
                includeImageLanguage: Globals.PreferredLanguage);

            return show;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get show from tmdb, name:[{name}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<TvSeason?> GetSeason(int showId, int seasonNr, string? apiKey)
    {
        try
        {
            TMDbClient client = new(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result = await client.GetTvSeasonAsync
            (
                tvShowId: showId,
                seasonNumber: seasonNr,
                language: Globals.PreferredLanguage,
                includeImageLanguage: Globals.PreferredLanguage
            );

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get season from tmdb, showID:[{showId}], seasonNr:[{seasonNr}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<TvEpisode?> GetEpisode(int showId, int seasonNr, int episodeNr, string? apiKey)
    {
        try
        {
            TMDbClient client = new(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result = await client.GetTvEpisodeAsync
            (
                tvShowId: showId,
                seasonNumber: seasonNr,
                episodeNumber: episodeNr,
                language: Globals.PreferredLanguage,
                includeImageLanguage: Globals.PreferredLanguage
            );

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get episode from tmdb, showID:[{showId}], seasonNr:[{seasonNr}], episodeNr:[{episodeNr}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<Person?> GetPerson(string name, string? apiKey)
    {
        try
        {
            TMDbClient client = new(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

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
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get person from tmdb, name:[{name}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<ImagesWithId?> GetMovieImages(int movieId, string? apiKey)
    {
        try
        {
            TMDbClient client = new TMDbClient(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result = await client.GetMovieImagesAsync(movieId);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get movie iamges from tmbd, movieID:[{movieId}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<ImagesWithId?> GetShowImages(int showId, string? apiKey)
    {
        try
        {
            TMDbClient client = new TMDbClient(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result = await client.GetTvShowImagesAsync(showId, language: Globals.PreferredLanguage,
                includeImageLanguage: Globals.PreferredLanguage);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get show iamges from tmbd, showID:[{showId}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<PosterImages?> GetSeasonImages(int showId, int seasonNr, string? apiKey)
    {
        try
        {
            TMDbClient client = new TMDbClient(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result = await client.GetTvSeasonImagesAsync(showId, seasonNr, language: Globals.PreferredLanguage);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get season iamges from tmbd, showID:[{showId}], seasonNr:[{seasonNr}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<StillImages?> GetEpisodeImages(int showId, int seasonNr, int episodeNr, string? apiKey)
    {
        try
        {
            TMDbClient client = new TMDbClient(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result =
                await client.GetTvEpisodeImagesAsync(showId, seasonNr, episodeNr, language: Globals.PreferredLanguage);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Failed to get episode iamges from tmbd, showID:[{showId}], seasonNr:[{seasonNr}], episodeNr:[{episodeNr}], message: {ex.Message}]");
            return null;
        }
    }

    public async Task<ProfileImages?> GetPersonImages(int personId, string? apiKey)
    {
        try
        {
            var client = new TMDbClient(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;

            var result = await client.GetPersonImagesAsync(personId);

            return result;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError($"failed to get person images from tmdb, personID: [{personId}] message: {e.Message}");
            return null;
        }
    }

    public async Task<byte[]?> GetImageFromId(string imagePath, string? apiKey)
    {
        try
        {

            TMDbClient client = new TMDbClient(apiKey);
            client.DefaultLanguage = Globals.PreferredLanguage;
            await client.GetConfigAsync();

            var result = await client.GetImageBytesAsync("original", imagePath, true);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(message: $"failed to receive image data from tmdb url:{imagePath}");
            return null;
        }
    }
}