using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.APIs;
using Shouldly;

namespace OpenMediaServer.Test.APIs;

public class GoogleBooksApiShould
{
    public GoogleBooksApi Api { get; set; }

    public GoogleBooksApiShould()
    {
        Api = new GoogleBooksApi(logger: Substitute.For<ILogger<GoogleBooksApi>>(), httpClient: new HttpClient());
    }

#if DEBUG

    [Fact]
    public async Task GetMetadata()
    {
        var model = await Api.GetBookMetadata
        (
            title: "Die Tribute von Panem - Tödliche Spiele"
        );

        model.ShouldNotBeNull();
    }

# endif

}
