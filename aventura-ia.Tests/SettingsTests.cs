namespace aventura_ia.Tests;

public class SettingsTests
{
    [Fact]
    public void ValidateChat_ThrowsWhenEndpointIsMissing()
    {
        var settings = new Settings
        {
            AzureOpenAiApiKey = "key",
            AzureOpenAiDeploymentId = "deployment"
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(settings.ValidateChat);

        Assert.Contains("Settings:AzureOpenAiEndpoint", exception.Message);
    }

    [Fact]
    public void ValidateImage_ThrowsWhenTimeoutIsInvalid()
    {
        var settings = new Settings
        {
            DalleEndpoint = "https://example.openai.azure.com/",
            DalleApiKey = "key",
            DalleDeploymentId = "image",
            ImageGenerationTimeoutSeconds = 0
        };

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(settings.ValidateImage);

        Assert.Contains("Settings:ImageGenerationTimeoutSeconds", exception.Message);
    }

    [Fact]
    public void ValidateVideo_AcceptsValidSoraSettings()
    {
        var settings = new Settings
        {
            SoraEndpoint = "https://example.openai.azure.com/",
            SoraApiKey = "key",
            SoraDeploymentId = "sora",
            VideoRequestTimeoutSeconds = 60
        };

        settings.ValidateVideo();
    }
}
