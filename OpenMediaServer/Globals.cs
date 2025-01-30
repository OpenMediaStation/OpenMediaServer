using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenMediaServer;

public static class Globals
{
    public static string ConfigFolder { get; set; } = Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "/config";
    public static string MediaFolder { get; set; } = Environment.GetEnvironmentVariable("MEDIA_PATH") ?? "/media";
    public static string Domain { get; set; } = Environment.GetEnvironmentVariable("DOMAIN") != null ? "https://" + Environment.GetEnvironmentVariable("DOMAIN") : "http://localhost:8080";
    
    public static string? AuthConfigurationUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_CONFIGURATION_URL");
    public static string AuthIssuer { get; set; } = Environment.GetEnvironmentVariable("AUTH_ISSUER") ?? (AuthConfigurationUrl != null ? string.Empty: throw new ArgumentException("AuthIssuer must be set"));
    public static string AuthorizeUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_AUTHORIZE") ?? (AuthConfigurationUrl != null ? string.Empty: throw new ArgumentException("AuthorizeUrl must be set"));
    public static string DeviceCodeUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_DEVICECODE") ?? (AuthConfigurationUrl != null ? string.Empty: throw new ArgumentException("DeviceCodeUrl must be set"));
    public static string TokenUrl { get; set; } = Environment.GetEnvironmentVariable("AUTH_TOKEN") ?? (AuthConfigurationUrl != null ? string.Empty: throw new ArgumentException("TokenUrl must be set"));
    public static string ClientId { get; set; } = Environment.GetEnvironmentVariable("AUTH_CLIENTID") ?? throw new ArgumentException("ClientId must be set");
    
    public static string OmdbApiKey { get; set; }
    public static string TmdbApiKey { get; set; }

    public static JsonSerializerOptions JsonOptions { get; set; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public static string? GetUserId(HttpContext httpContext)
    {
        return httpContext.User.FindFirst("sub")?.Value ??
               httpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    }

    public static string GetUserStorage(HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);

        if (userId == null)
        {
           throw new ArgumentNullException(userId);
        }

        return Path.Combine(ConfigFolder, "users", userId);
    }

    public static string GetUserStorage(string? userId)
    {
        if (userId == null)
        {
           throw new ArgumentNullException(userId);
        }

        return Path.Combine(ConfigFolder, "users", userId);
    }
}
