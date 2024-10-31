using System;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMediaServer.APIs;
using Shouldly;

namespace OpenMediaServer.Test.APIs;

public class OMDbAPIShould
{
    public OMDbAPI Api { get; set; }

    public OMDbAPIShould()
    {
        Setup.Configure();

        var logger = new Mock<ILogger<OMDbAPI>>();
        Api = new OMDbAPI(logger: logger.Object, httpClient: new HttpClient());
    }

#if DEBUG

    [Fact]
    public async Task GetMetadata()
    {
        var model = await Api.GetMetadata
        (
            name: "Lucifer", 
            apiKey: "2b55c18b",
            season: 1,
            episode: 1
        );

        model.ShouldNotBeNull();
    }

# endif

}
