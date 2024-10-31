using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenMediaServer;

public static class Globals
{
    public static string ConfigFolder { get; set; } = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "/config";
    public static string MediaFolder { get; set; } = Environment.GetEnvironmentVariable("MEDIA_PATH") ?? "/media";
    public static string Domain { get; set; } = Environment.GetEnvironmentVariable("DOMAIN") != null ? "https://" + Environment.GetEnvironmentVariable("DOMAIN") : "http://localhost:8080";
    
    public static string AuthIssuer { get; set; } = Environment.GetEnvironmentVariable("AUTH_ISSUER") ?? throw new ArgumentException("AuthIssuer must be set");
    public static string AuthorizeUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_AUTHORIZE") ?? throw new ArgumentException("AuthorizeUrl must be set");
    public static string DeviceCodeUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_DEVICECODE") ?? throw new ArgumentException("DeviceCodeUrl must be set");
    public static string TokenUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_TOKEN") ?? throw new ArgumentException("TokenUrl must be set");
    public static string ClientId { get; set; } = Environment.GetEnvironmentVariable("AUTH_CLIENTID") ?? throw new ArgumentException("ClientId must be set");
    
    public static JsonSerializerOptions JsonOptions { get; set; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };
}
