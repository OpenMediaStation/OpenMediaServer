using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.Services;

public interface IDiscoveryShowService
{
    Task CreateShow(string path);
}
