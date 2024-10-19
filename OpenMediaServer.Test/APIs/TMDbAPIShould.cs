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
    public async Task GetMovieMetadata()
    {
        var model = await Api.GetMovie
        (
            name: "Die Tribute von Panem",
            apiKey: "f8cd15aa25675794601f72aeed118f02"
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetShowMetadata()
    {
        var model = await Api.GetShow
        (
            name: "Lucifer",
            apiKey: "f8cd15aa25675794601f72aeed118f02"
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetPersonMetadata()
    {
        var model = await Api.GetPerson
        (
            name: "Bob Marley",
            apiKey: "f8cd15aa25675794601f72aeed118f02"
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetImages()
    {
        var model = await Api.GetMovieImages
        (
            movieId: 70160,
            apiKey: "f8cd15aa25675794601f72aeed118f02"
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetImageBytes()
    {
        var bytes = await Api.GetImageFromId
        (
            imagePath: "/4gSdZvuUzLIlXdafjmG9HFMIWwm.jpg",
            apiKey: "f8cd15aa25675794601f72aeed118f02"
        );

        File.WriteAllBytes("./test.jpg", bytes);

        bytes.ShouldNotBeNull();
    }

# endif
}
