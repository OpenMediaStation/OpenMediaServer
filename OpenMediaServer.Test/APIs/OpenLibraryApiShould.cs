using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.APIs;
using Shouldly;

namespace OpenMediaServer.Test.APIs;

public class OpenLibraryApiShould
{
    public OpenLibraryApi Api { get; set; }

    public OpenLibraryApiShould()
    {
        Setup.Configure();

        Api = new OpenLibraryApi(logger: Substitute.For<ILogger<OpenLibraryApi>>(), httpClient: new HttpClient());
    }

#if DEBUG

    [Fact]
    public async Task GetMetadata()
    {
        var model = await Api.SearchBook
        (
            title: "Das Känguru Manifest",
            isAudiobook: true,
            language: "de"
        );

        var openLibraryBookDetails = await Api.GetBookDetails(model.Docs.First().Key);

        model.ShouldNotBeNull();
    }

# endif
}
