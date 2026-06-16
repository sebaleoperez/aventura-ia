public sealed class Settings
{
    public string AzureOpenAiEndpoint { get; set; } = string.Empty;
    public string AzureOpenAiApiKey { get; set; } = string.Empty;
    public string AzureOpenAiDeploymentId { get; set; } = string.Empty;
    public string DalleEndpoint { get; set; } = string.Empty;
    public string DalleApiKey { get; set; } = string.Empty;
    public string DalleDeploymentId { get; set; } = string.Empty;
    public string SoraEndpoint { get; set; } = string.Empty;
    public string SoraApiKey { get; set; } = string.Empty;
    public string SoraDeploymentId { get; set; } = string.Empty;
    public int ImageGenerationTimeoutSeconds { get; set; } = 120;
    public int VideoRequestTimeoutSeconds { get; set; } = 60;
    public bool EnableVideoPolling { get; set; } = false;
    public int VideoPollingIntervalSeconds { get; set; } = 10;
    public int VideoPollingTimeoutSeconds { get; set; } = 600;

    public void ValidateChat()
    {
        RequireUri(AzureOpenAiEndpoint, nameof(AzureOpenAiEndpoint));
        Require(AzureOpenAiApiKey, nameof(AzureOpenAiApiKey));
        Require(AzureOpenAiDeploymentId, nameof(AzureOpenAiDeploymentId));
    }

    public void ValidateImage()
    {
        RequireUri(DalleEndpoint, nameof(DalleEndpoint));
        Require(DalleApiKey, nameof(DalleApiKey));
        Require(DalleDeploymentId, nameof(DalleDeploymentId));
        RequirePositive(ImageGenerationTimeoutSeconds, nameof(ImageGenerationTimeoutSeconds));
    }

    public void ValidateVideo()
    {
        RequireUri(SoraEndpoint, nameof(SoraEndpoint));
        Require(SoraApiKey, nameof(SoraApiKey));
        Require(SoraDeploymentId, nameof(SoraDeploymentId));
        RequirePositive(VideoRequestTimeoutSeconds, nameof(VideoRequestTimeoutSeconds));
        RequirePositive(VideoPollingIntervalSeconds, nameof(VideoPollingIntervalSeconds));
        RequirePositive(VideoPollingTimeoutSeconds, nameof(VideoPollingTimeoutSeconds));
    }

    private static void Require(string value, string settingName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required setting: Settings:{settingName}.");
        }
    }

    private static void RequireUri(string value, string settingName)
    {
        Require(value, settingName);

        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"Invalid URI in setting: Settings:{settingName}.");
        }
    }

    private static void RequirePositive(int value, string settingName)
    {
        if (value <= 0)
        {
            throw new InvalidOperationException($"Setting Settings:{settingName} must be greater than zero.");
        }
    }
}
