using System.Reflection;
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

// Configure logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.File("/config/logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

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

builder.Services.AddSingleton<IContentDiscoveryService, ContentDiscoveryService>();
builder.Services.AddSingleton<IFileSystemRepository, FileSystemRepository>();
builder.Services.AddSingleton<IInventoryService, InventoryService>();
builder.Services.AddSingleton<IStreamingService, StreamingService>();
builder.Services.AddSingleton<IOmdbAPI, OMDbAPI>();
builder.Services.AddSingleton<IGeneralApiEndpoints, GeneralApiEndpoints>();
builder.Services.AddSingleton<IMetadataEndpoints, MetadataEndpoints>();
builder.Services.AddSingleton<IStreamingEndpoints, StreamingEndpoints>();
builder.Services.AddSingleton<IFileInfoEndpoints, FileInfoEndpoints>();
builder.Services.AddSingleton<IInventoryEndpoints, InventoryEndpoints>();
builder.Services.AddSingleton<IMetadataService, MetadataService>();
builder.Services.AddSingleton<IFileInfoService, FileInfoService>();

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

app.Services.GetService<IGeneralApiEndpoints>()?.Map(app);
app.Services.GetService<IStreamingEndpoints>()?.Map(app);
app.Services.GetService<IMetadataEndpoints>()?.Map(app);
app.Services.GetService<IFileInfoEndpoints>()?.Map(app);
app.Services.GetService<IInventoryEndpoints>()?.Map(app);

app.Lifetime.ApplicationStopping.Register(() => Log.Information("Application shutting down"));

app.Run();
