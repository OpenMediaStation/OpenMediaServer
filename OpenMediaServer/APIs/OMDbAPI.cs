using System;
using System.Web;
using OpenMediaServer.DTOs;
using OpenMediaServer.Extensions;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Models;

namespace OpenMediaServer.APIs;

public class OMDbAPI : IMetadataAPI
{
    private readonly ILogger<OMDbAPI> _logger;
    private readonly HttpClient _httpClient;

    public OMDbAPI(ILogger<OMDbAPI> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<MovieShowMetadataModel?> GetMetadata(string name, string? apiKey, bool fullPlot = false, string? year = null, string type = "movie")
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
        query["type"] = type; //Accepted types: movie, series, episode
        if (!string.IsNullOrEmpty(year))
            query["year"] = year;
        
        var message = await _httpClient.GetAsync($"http://www.omdbapi.com/?{query}");

        if (message.IsSuccessStatusCode)
        {
            var model = await message.Content.ReadFromJsonAsync<OMDbModel>();

            return model?.ToMetadataItem();
        }
        else
        {
            _logger.LogWarning("OMDb could not retrieve metadata with error: {ErrorCode} and message: {Message}", message.StatusCode, message.ReasonPhrase);
        }

        return null;
    }
}
