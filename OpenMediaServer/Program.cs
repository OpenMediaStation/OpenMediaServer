using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenMediaServer;
using OpenMediaServer.APIs;
using OpenMediaServer.Endpoints;
using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Repositories;
using OpenMediaServer.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with appsettings.json configuration
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
          .Enrich.FromLogContext();
});

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

var info = new OpenApiInfo()
{
    Title = "OpenMediaServer",
    Version = "v1",
    Description = "Description of your API"
};

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", info);

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Register application services and endpoints
RegisterServices(builder.Services);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
                     .AllowAnyHeader()
                     .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = Globals.AuthConfigurationUrl ?? Globals.AuthIssuer;
    options.Audience = Globals.ClientId;
    options.RequireHttpsMetadata = true;

    if (Globals.AuthConfigurationUrl != null) //Get configuration and initialize the Globals
    {
        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(Globals.AuthConfigurationUrl, new OpenIdConnectConfigurationRetriever());
        options.Configuration = options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
        Globals.AuthIssuer = options.Configuration?.Issuer ?? throw new ArgumentException("AuthIssuer configuration failed. Please verify the .well-known endpoint.");
        Globals.AuthorizeUrl =  options.Configuration?.AuthorizationEndpoint ?? throw new ArgumentException("AuthorizeUrl configuration failed. Please verify the .well-known endpoint.");
        Globals.DeviceCodeUrl = options.Configuration?.DeviceAuthorizationEndpoint ?? throw new ArgumentException("DeviceCodeUrl configuration failed. Please verify the .well-known endpoint.");
        Globals.TokenUrl =  options.Configuration?.TokenEndpoint ?? throw new ArgumentException("TokenUrl configuration failed. Please verify the .well-known endpoint.");
    }
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Globals.AuthIssuer,
        ValidateAudience = true,
        ValidAudience = Globals.ClientId,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidateIssuerSigningKey = true,
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Set ApiKeys
var configuration = app.Services.GetRequiredService<IConfiguration>();
Globals.OmdbApiKey = Environment.GetEnvironmentVariable("OMDB_KEY") ?? configuration.GetValue<string>("OpenMediaServer:OMDbKey") ?? throw new ArgumentException("OmdbKey missing");
Globals.TmdbApiKey = Environment.GetEnvironmentVariable("TMDB_KEY") ?? configuration.GetValue<string>("OpenMediaServer:TMDBKey") ?? throw new ArgumentException("TmdbKey missing");

// Configure middleware
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Run initial content scan and set up watchers
var contentDiscoveryService = app.Services.GetRequiredService<IContentDiscoveryService>();
await contentDiscoveryService.ActiveScan(Globals.MediaFolder);
contentDiscoveryService.Watch(Globals.MediaFolder);

// Map endpoints
MapEndpoints(app);

app.Lifetime.ApplicationStopping.Register(() => Log.Information("Application shutting down"));
app.Run();

// Method to register services
void RegisterServices(IServiceCollection services)
{
    services.AddSingleton<IContentDiscoveryService, ContentDiscoveryService>();
    services.AddSingleton<IFileSystemRepository, FileSystemRepository>();
    services.AddSingleton<IInventoryService, InventoryService>();
    services.AddSingleton<IStreamingService, StreamingService>();
    services.AddSingleton<IOmdbAPI, OMDbAPI>();
    services.AddSingleton<IGeneralApiEndpoints, GeneralApiEndpoints>();
    services.AddSingleton<IMetadataEndpoints, MetadataEndpoints>();
    services.AddSingleton<IProgressEndpoints, ProgressEndpoints>();
    services.AddSingleton<IStreamingEndpoints, StreamingEndpoints>();
    services.AddSingleton<IFileInfoEndpoints, FileInfoEndpoints>();
    services.AddSingleton<IAddonEndpoints, AddonEndpoints>();
    services.AddSingleton<IFavoriteEndpoints, FavoriteEndpoints>();
    services.AddSingleton<IImageEndpoints, ImageEndpoints>();
    services.AddSingleton<IInventoryEndpoints, InventoryEndpoints>();
    services.AddSingleton<IMetadataService, MetadataService>();
    services.AddSingleton<IFileInfoService, FileInfoService>();
    services.AddSingleton<IDiscoveryShowService, DiscoveryShowService>();
    services.AddSingleton<IDiscoveryMovieService, DiscoveryMovieService>();
    services.AddSingleton<IDiscoveryBookService, DiscoveryBookService>();
    services.AddSingleton<IGoogleBooksApi, GoogleBooksApi>();
    services.AddSingleton<ITMDbAPI, TMDbAPI>();
    services.AddSingleton<IImageService, ImageService>();
    services.AddSingleton<IAddonService, AddonService>();
    services.AddSingleton<IProgressService, ProgressService>();
    services.AddSingleton<IBinService, BinService>();

    services.AddHttpClient<IOmdbAPI, OMDbAPI>();
}

// Method to map endpoints
void MapEndpoints(WebApplication app)
{
    app.Services.GetRequiredService<IGeneralApiEndpoints>().Map(app);
    app.Services.GetRequiredService<IStreamingEndpoints>().Map(app);
    app.Services.GetRequiredService<IMetadataEndpoints>().Map(app);
    app.Services.GetRequiredService<IFileInfoEndpoints>().Map(app);
    app.Services.GetRequiredService<IInventoryEndpoints>().Map(app);
    app.Services.GetRequiredService<IImageEndpoints>().Map(app);
    app.Services.GetRequiredService<IAddonEndpoints>().Map(app);
    app.Services.GetRequiredService<IFavoriteEndpoints>().Map(app);
    app.Services.GetRequiredService<IProgressEndpoints>().Map(app);
}
