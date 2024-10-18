using System;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMediaServer.APIs;
using Shouldly;

namespace OpenMediaServer.Test.APIs;

public class TMDbAPIShould
{
    public TMDbAPI Api { get; set; }

    public TMDbAPIShould()
    {
        var logger = new Mock<ILogger<TMDbAPI>>();
        Api = new TMDbAPI(logger: logger.Object, httpClient: new HttpClient());
    }

#if DEBUG

    [Fact]
    public async Task GetMetadata()
    {
        var model = await Api.GetMovie
        (
            name: "Die Tribute von Panem", 
            apiKey: "f8cd15aa25675794601f72aeed118f02"
        );

        model.ShouldNotBeNull();
    }

# endif
}
