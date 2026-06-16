using Microsoft.Extensions.Configuration;

public static class ConfigurationHelper
{
    public static Settings LoadSettings()
    {
        IConfigurationRoot config = BuildConfiguration();
        var settings = config.GetSection("Settings").Get<Settings>() ?? new Settings();
        settings.ValidateChat();
        return settings;
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        string environmentName =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddUserSecrets<OpenAIService>(optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
