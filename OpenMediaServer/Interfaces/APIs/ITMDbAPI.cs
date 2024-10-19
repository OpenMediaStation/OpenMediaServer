using TMDbLib.Objects.General;
using TMDbLib.Objects.Movies;
using TMDbLib.Objects.People;
using TMDbLib.Objects.TvShows;

namespace OpenMediaServer.Interfaces.APIs;

public interface ITMDbAPI
{
    Task<Movie?> GetMovie(string name, string? apiKey, string? year = null);
    Task<ImagesWithId?> GetMovieImages(int movieId, string? apiKey);
    Task<byte[]?> GetImageFromId(string imagePath, string? apiKey);
    Task<TvShow?> GetShow(string name, string? apiKey, string? year = null);
    Task<Person?> GetPerson(string name, string? apiKey);
    Task<ImagesWithId?> GetShowImages(int showId, string? apiKey);
    Task<ProfileImages?> GetPersonImages(int personId, string? apiKey);
    Task<PosterImages?> GetSeasonImages(int showId, int seasonNr, string? apiKey);
    Task<StillImages?> GetEpisodeImages(int showId, int seasonNr, int episodeNr, string? apiKey);
}
