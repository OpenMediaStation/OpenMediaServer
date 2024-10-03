using System;
using OpenMediaServer.DTOs;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.APIs;

public interface IOmdbAPI
{
    Task<OMDbModel?> GetMetadata(string name, string? apiKey, bool fullPlot = false, string? year = null, string? type = null, int? season = null, int? episode = null);
}
