using System.Web;
using OpenMediaServer.DTOs;
using OpenMediaServer.Interfaces.APIs;

namespace OpenMediaServer.APIs;

public class OMDbAPI : IOmdbAPI
{
    private readonly ILogger<OMDbAPI> _logger;
    private readonly HttpClient _httpClient;

    public OMDbAPI(ILogger<OMDbAPI> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<OMDbModel?> GetMetadata(string name, string? apiKey, bool fullPlot = false, string? year = null, string? type = null, int? season = null, int? episode = null)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("ApiKey null or empty. Cannot use OMDbAPI.");
            return null;
        }

        string plot = "short";

        if (fullPlot)
        {
            plot = "full";
        }

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["apikey"] = apiKey;
        query["t"] = name;
        query["plot"] = plot;
        query["season"] = season.ToString();
        query["episode"] = episode.ToString();
        query["type"] = type; //Accepted types: movie, series, episode
        if (!string.IsNullOrEmpty(year))
            query["year"] = year;
        
        var url = $"http://www.omdbapi.com/?{query}";

        var message = await _httpClient.GetAsync(url);

        if (message.IsSuccessStatusCode)
        {
            var model = await message.Content.ReadFromJsonAsync<OMDbModel>();

            return model;
        }
        else
        {
            _logger.LogWarning("OMDb could not retrieve metadata with error: {ErrorCode} and message: {Message}", message.StatusCode, message.ReasonPhrase);
        }

        return null;
    }
}
