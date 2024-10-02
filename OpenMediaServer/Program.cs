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

// Configure logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.File("/config/logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IContentDiscoveryService, ContentDiscoveryService>();
builder.Services.AddSingleton<IStorageRepository, FileSystemRepository>();
builder.Services.AddSingleton<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IStreamingService, StreamingService>();
builder.Services.AddSingleton<IOmdbAPI, OMDbAPI>();
builder.Services.AddSingleton<IApiEndpoints, ApiEndpoints>();
builder.Services.AddSingleton<IMetadataEndpoints, MetadataEndpoints>();
builder.Services.AddSingleton<IStreamingEndpoints, StreamingEndpoints>();
builder.Services.AddSingleton<IMetadataService, MetadataService>();

builder.Services.AddHttpClient<IOmdbAPI, OMDbAPI>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var contentDiscoveryService = app.Services.GetService<IContentDiscoveryService>();
await contentDiscoveryService?.ActiveScan(Globals.MediaFolder);
contentDiscoveryService?.Watch(Globals.MediaFolder);

app.Services.GetService<IApiEndpoints>()?.Map(app);
app.Services.GetService<IStreamingEndpoints>()?.Map(app);
app.Services.GetService<IMetadataEndpoints>()?.Map(app);

app.Lifetime.ApplicationStopping.Register(() => Log.Information("Application shutting down"));

app.Run();
