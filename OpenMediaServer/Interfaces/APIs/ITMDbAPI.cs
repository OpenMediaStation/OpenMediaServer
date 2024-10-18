using TMDbLib.Objects.Movies;

namespace OpenMediaServer.Interfaces.APIs;

public interface ITMDbAPI
{
    Task<Movie?> GetMovie(string name, string? apiKey, string? year = null);
}
