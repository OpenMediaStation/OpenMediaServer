using System;

namespace OpenMediaServer.Test;

public static class Setup
{
    public static void Configure()
    {
        Environment.SetEnvironmentVariable("AUTH_ISSUER", "There is nothing");
        Environment.SetEnvironmentVariable("AUTH_AUTHORIZE", "There is nothing");
        Environment.SetEnvironmentVariable("AUTH_DEVICECODE", "There is nothing");
        Environment.SetEnvironmentVariable("AUTH_TOKEN", "There is nothing");
        Environment.SetEnvironmentVariable("AUTH_CLIENTID", "There is nothing");
        Environment.SetEnvironmentVariable("PG_HOST", "There is nothing");
        Environment.SetEnvironmentVariable("PG_USER", "There is nothing");
        Environment.SetEnvironmentVariable("PG_PASSWORD", "There is nothing");
    }
}
