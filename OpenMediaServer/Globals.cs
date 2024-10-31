using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenMediaServer;

public static class Globals
{
    public static string ConfigFolder { get; set; } = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "/config";
    public static string MediaFolder { get; set; } = Environment.GetEnvironmentVariable("MEDIA_PATH") ?? "/media";
    public static string Domain { get; set; } = Environment.GetEnvironmentVariable("DOMAIN") != null ? "https://" + Environment.GetEnvironmentVariable("DOMAIN") : "http://localhost:8080";
    public static string AuthIssuer { get; set; } = Environment.GetEnvironmentVariable("AUTHISSUER") ?? throw new ArgumentException("AuthIssuer must be set");
    
    public static JsonSerializerOptions JsonOptions { get; set; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };
}
