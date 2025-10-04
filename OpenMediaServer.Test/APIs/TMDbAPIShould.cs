using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OpenMediaServer.APIs;
using Shouldly;

namespace OpenMediaServer.Test.APIs;

public class TMDbAPIShould
{
    
    private IConfigurationRoot _config = new ConfigurationBuilder()
        .AddUserSecrets<TMDbAPIShould>()
        .Build();
    
    public TMDbAPI Api { get; set; }

    public TMDbAPIShould()
    {
        Setup.Configure();

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
            apiKey: _config["tmdb_api_key"]
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetShowMetadata()
    {
        var model = await Api.GetShow
        (
            name: "Lucifer",
            apiKey: _config["tmdb_api_key"]
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetPersonMetadata()
    {
        var model = await Api.GetPerson
        (
            name: "Bob Marley",
            apiKey: _config["tmdb_api_key"]
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetImages()
    {
        var model = await Api.GetMovieImages
        (
            movieId: 70160,
            apiKey: _config["tmdb_api_key"]
        );

        model.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetImageBytes()
    {
        var bytes = await Api.GetImageFromId
        (
            imagePath: "/4gSdZvuUzLIlXdafjmG9HFMIWwm.jpg",
            apiKey: _config["tmdb_api_key"]
        );

        File.WriteAllBytes("./test.jpg", bytes);

        bytes.ShouldNotBeNull();
    }

# endif
}
