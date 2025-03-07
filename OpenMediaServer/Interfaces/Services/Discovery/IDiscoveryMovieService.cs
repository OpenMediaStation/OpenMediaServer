using System;

namespace OpenMediaServer.Interfaces.Services;

public interface IDiscoveryMovieService
{
    Task CreateMovie(string path);
}
