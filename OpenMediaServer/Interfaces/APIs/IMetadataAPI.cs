using System;
using OpenMediaServer.Models;

namespace OpenMediaServer.Interfaces.APIs;

public interface IMetadataAPI
{
    public Task<MovieShowMetadataModel?> GetMetadata(string name, string? apiKey, bool fullPlot = false);
}
