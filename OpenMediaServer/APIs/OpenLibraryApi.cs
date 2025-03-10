using System.Text.Json;
using OpenMediaServer.DTOs;
using OpenMediaServer.Interfaces.APIs;

namespace OpenMediaServer.APIs;

public class OpenLibraryApi : IOpenLibraryApi
{
    private readonly ILogger<OpenLibraryApi> _logger;
    private readonly HttpClient _httpClient;
    private const string OpenLibraryBaseUrl = "https://openlibrary.org";

    public OpenLibraryApi(ILogger<OpenLibraryApi> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<OpenLibrarySearchResult?> SearchBook(string title, bool isAudiobook = false, string language = "en")
    {
        if (string.IsNullOrEmpty(title))
        {
            _logger.LogWarning("Title is null or empty. Cannot search Open Library API.");
            return null;
        }

        try
        {
            string query = $"{OpenLibraryBaseUrl}/search.json?q={Uri.EscapeDataString(title)}&lang={language}&limit=1";
            HttpResponseMessage response = await _httpClient.GetAsync(query);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve data from Open Library API. Status Code: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseText = await response.Content.ReadAsStringAsync();
            var searchResult = JsonSerializer.Deserialize<OpenLibrarySearchResult>(responseText, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            if (searchResult?.Docs == null || searchResult.Docs.Count == 0)
            {
                _logger.LogWarning("No results found for the title: {Title}", title);
                return null;
            }

            return searchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching audiobook metadata from Open Library API.");
            return null;
        }
    }

    public string? GetCover(bool isAudiobook, BookDoc? bookData, IEnumerable<BookEdition>? editions)
    {
        var cover = bookData?.CoverID != null ? $"https://covers.openlibrary.org/b/id/{bookData.CoverID}-L.jpg" : null;

        if (!isAudiobook)
        {
            return cover;
        }

        var filteredEditions = editions?.Where(edition => edition.PhysicalFormat?.ToLower().Contains("audio") ?? false);
        var coverId = filteredEditions?.FirstOrDefault()?.Covers?.FirstOrDefault();

        cover = bookData?.CoverID != null ? $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg" : null;

        return cover;
    }

    public async Task<OpenLibraryWorkDetails?> GetBookDetails(string? workKey)
    {
        try
        {
            string detailsUrl = $"{OpenLibraryBaseUrl}{workKey}.json";
            HttpResponseMessage response = await _httpClient.GetAsync(detailsUrl);

            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync();
            var bookDetails = JsonSerializer.Deserialize<OpenLibraryWorkDetails>(responseString);

            return bookDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve book description.");
            return null;
        }
    }

    public async Task<BookEditionsResponse?> GetWorks(string? workKey)
    {
        try
        {
            string detailsUrl = $"{OpenLibraryBaseUrl}{workKey}/editions.json";
            HttpResponseMessage response = await _httpClient.GetAsync(detailsUrl);

            if (!response.IsSuccessStatusCode) return null;

            var responseString = await response.Content.ReadAsStringAsync();
            var edition = JsonSerializer.Deserialize<BookEditionsResponse>(responseString);

            return edition;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve book description.");
            return null;
        }
    }
}