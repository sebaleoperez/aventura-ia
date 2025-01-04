public sealed class Settings
{
    public required string AzureOpenAiEndpoint { get; set; }
    public required string AzureOpenAiApiKey { get; set; }
    public required string AzureOpenAiDeploymentId { get; set; } = null!;
    public required string DalleEndpoint { get; set; } = null!;
    public required string DalleApiKey { get; set; } = null!;
    public required string DalleDeploymentId { get; set; } = null!;
}