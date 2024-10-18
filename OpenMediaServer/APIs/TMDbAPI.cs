using System;
using OpenMediaServer.DTOs;

namespace OpenMediaServer.APIs;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMediaServer.Interfaces.APIs;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

public class TMDbAPI : ITMDbAPI
{
    private readonly ILogger<TMDbAPI> _logger;
    private readonly HttpClient _httpClient;

    // TMDb base API URL
    private const string TMDbBaseUrl = "https://api.themoviedb.org/3/";

    public TMDbAPI(ILogger<TMDbAPI> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Movie?> GetMovie(string name, string? apiKey, string? year = null)
    {
        TMDbClient client = new TMDbClient(apiKey);

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

    // public async Task<Movie?> GetShow(string name, string? apiKey, string? year = null, string? type = null, int? season = null, int? episode = null) { }

}