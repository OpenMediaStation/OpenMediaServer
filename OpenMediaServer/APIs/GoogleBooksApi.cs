using System.Web;
using OpenMediaServer.DTOs; 
using OpenMediaServer.Interfaces.APIs;

namespace OpenMediaServer.APIs
{
    public class GoogleBooksApi : IGoogleBooksApi
    {
        private readonly ILogger<GoogleBooksApi> _logger;
        private readonly HttpClient _httpClient;

        public GoogleBooksApi(ILogger<GoogleBooksApi> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<GoogleBooksModel?> GetBookMetadata(string title, string? apiKey = null, int? maxResults = 1, string? language = null, string? printType = "books")
        {
            if (string.IsNullOrEmpty(title))
            {
                _logger.LogWarning("Query is null or empty. Cannot search Google Books API.");
                return null;
            }

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["q"] = title;
            if (!string.IsNullOrEmpty(apiKey))
                queryString["key"] = apiKey;
            if (maxResults.HasValue)
                queryString["maxResults"] = maxResults.Value.ToString();
            if (!string.IsNullOrEmpty(language))
                queryString["langRestrict"] = language;
            if (!string.IsNullOrEmpty(printType))
                queryString["printType"] = printType;

            var url = $"https://www.googleapis.com/books/v1/volumes?{queryString}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var model = await response.Content.ReadFromJsonAsync<GoogleBooksModel>();

                return model;
            }
            else
            {
                _logger.LogWarning("Google Books API could not retrieve metadata. Status: {StatusCode}, Message: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }
    }
}
