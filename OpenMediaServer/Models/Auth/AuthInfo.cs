using System;

namespace OpenMediaServer.Models.Auth;

public class AuthInfo
{
    public string AuthorizeUrl { get; set; }
    public string DeviceCodeUrl { get; set; }
    public string TokenUrl { get; set; }
    public string ClientId { get; set; }
}
