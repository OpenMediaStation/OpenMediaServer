using System;
using Microsoft.Extensions.FileProviders;

namespace OpenMediaServer.Interfaces.Endpoints;

public interface IFileInfoEndpoints
{
    public void Map(WebApplication app);
}
