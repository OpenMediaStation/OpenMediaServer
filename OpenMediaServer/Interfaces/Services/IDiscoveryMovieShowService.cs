using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IDiscoveryMovieShowService
{
    Task CreateMovie(string path);
    Task CreateShow(string path);
}
