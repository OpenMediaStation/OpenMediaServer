using System;
using OpenMediaServer.DTOs;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.APIs;

public interface IOmdbAPI
{
    public Task<OMDbModel?> GetMetadata(string name, string? apiKey, bool fullPlot = false, string? year = null, string type = "");
}
