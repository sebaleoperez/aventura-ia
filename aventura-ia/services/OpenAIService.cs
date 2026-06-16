using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using System.Diagnostics;
using System.Text.Json;

public class OpenAIService : IAiAdventureService {
    readonly Settings _settings;
    readonly AzureOpenAIClient _client;
    readonly ChatClient _chatClient;
    readonly HttpClient _httpClient;
    ImageClient? _imageClient;
    readonly List<ChatMessage> _messages = new List<ChatMessage>();

    public OpenAIService(Settings settings, HttpClient httpClient) {
        _settings = settings;
        _httpClient = httpClient;

        _client = new AzureOpenAIClient(
            new Uri(_settings.AzureOpenAiEndpoint),
            new AzureKeyCredential(_settings.AzureOpenAiApiKey)
        );

        _chatClient = _client.GetChatClient(_settings.AzureOpenAiDeploymentId);
    }

    public async Task<Uri> GetImageGenerationsAsync(string? scenario, string? graphics) {
        // Get image generation for the first part of the game
        string prompt = "Give me an image of: " + scenario + ". I want the image to follow this type: " + graphics;
        ImageClient imageClient = GetImageClient();
        ImageGenerationOptions options = new()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Vivid,
            ResponseFormat = GeneratedImageFormat.Uri
        };
        GeneratedImage image = await imageClient
            .GenerateImageAsync(prompt, options)
            .WaitAsync(TimeSpan.FromSeconds(_settings.ImageGenerationTimeoutSeconds));

        // Image Generations responses provide URLs you can use to retrieve requested images
        return image.ImageUri;
    }

    public async Task<string> GetChatCompletionsAsync(string? input) {
        _messages.Add(new UserChatMessage(input));
        ChatCompletion completion = await _chatClient.CompleteChatAsync(_messages);
        string responseText = completion.Content[0].Text;
        _messages.Add(new AssistantChatMessage(responseText));
        return responseText;
    }

    public async Task<Translations> GetTranslationsAsync(string language) {
        string initialInput = GameHelper.GetTranslationsInput(language);

        return GameHelper.ParseTranslationsResponse(await GetSingleChatCompletionAsync(initialInput));
    }

    private async Task<string> GetSingleChatCompletionAsync(string input)
    {
        ChatCompletion completion = await _chatClient.CompleteChatAsync(new List<ChatMessage>
        {
            new UserChatMessage(input)
        });

        return completion.Content[0].Text;
    }

    public async Task<string> GenerateVideoAsync(string scenario) {
        _settings.ValidateVideo();

        string videoPrompt = $"Create a cinematic video scene of: {scenario}. The scene should be atmospheric, detailed, and immersive, showing the environment and setting in a way that brings the adventure scenario to life.";

        var requestBody = new {
            model = _settings.SoraDeploymentId,
            prompt = videoPrompt,
            height = "1080",
            width = "1920",
            n_seconds = "10",
            n_variants = "1"
        };

        string jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        string apiUrl = $"{_settings.SoraEndpoint.TrimEnd('/')}/openai/v1/video/generations/jobs?api-version=preview";

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.VideoRequestTimeoutSeconds));
        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = content
        };
        request.Headers.Add("api-key", _settings.SoraApiKey);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, timeout.Token);
        string responseContent = await response.Content.ReadAsStringAsync(timeout.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Sora API error ({response.StatusCode}): {responseContent}");
        }

        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

        if (jsonResponse.TryGetProperty("id", out var jobIdProperty)) {
            string jobId = jobIdProperty.GetString() ?? "";
            return _settings.EnableVideoPolling
                ? await WaitForVideoJobAsync(jobId)
                : jobId;
        }

        return responseContent;
    }

    private async Task<string> WaitForVideoJobAsync(string jobId)
    {
        string statusUrl = $"{_settings.SoraEndpoint.TrimEnd('/')}/openai/v1/video/generations/jobs/{jobId}?api-version=preview";
        TimeSpan pollingInterval = TimeSpan.FromSeconds(_settings.VideoPollingIntervalSeconds);
        TimeSpan pollingTimeout = TimeSpan.FromSeconds(_settings.VideoPollingTimeoutSeconds);
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < pollingTimeout)
        {
            await Task.Delay(pollingInterval);

            using var requestTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.VideoRequestTimeoutSeconds));
            using var request = new HttpRequestMessage(HttpMethod.Get, statusUrl);
            request.Headers.Add("api-key", _settings.SoraApiKey);

            using HttpResponseMessage response = await _httpClient.SendAsync(request, requestTimeout.Token);
            string responseContent = await response.Content.ReadAsStringAsync(requestTimeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Could not get video job status ({response.StatusCode}): {responseContent}");
            }

            JsonElement jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            string status = GetVideoJobStatus(jsonResponse);

            if (IsCompletedVideoStatus(status))
            {
                string? videoUrl = FindFirstStringProperty(jsonResponse, "url", "uri", "video_url", "download_url");
                return string.IsNullOrWhiteSpace(videoUrl) ? responseContent : videoUrl;
            }

            if (IsFailedVideoStatus(status))
            {
                throw new InvalidOperationException($"Video job failed: {responseContent}");
            }
        }

        return jobId;
    }

    private static string GetVideoJobStatus(JsonElement jsonResponse)
    {
        string? status = FindFirstStringProperty(jsonResponse, "status", "state");
        return string.IsNullOrWhiteSpace(status) ? "unknown" : status;
    }

    private static bool IsCompletedVideoStatus(string status)
    {
        return status.Equals("succeeded", StringComparison.OrdinalIgnoreCase)
            || status.Equals("completed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("success", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFailedVideoStatus(string status)
    {
        return status.Equals("failed", StringComparison.OrdinalIgnoreCase)
            || status.Equals("cancelled", StringComparison.OrdinalIgnoreCase)
            || status.Equals("canceled", StringComparison.OrdinalIgnoreCase);
    }

    private static string? FindFirstStringProperty(JsonElement element, params string[] propertyNames)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String
                    && propertyNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                {
                    return property.Value.GetString();
                }

                string? nestedValue = FindFirstStringProperty(property.Value, propertyNames);
                if (!string.IsNullOrWhiteSpace(nestedValue))
                {
                    return nestedValue;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                string? nestedValue = FindFirstStringProperty(item, propertyNames);
                if (!string.IsNullOrWhiteSpace(nestedValue))
                {
                    return nestedValue;
                }
            }
        }

        return null;
    }

    private ImageClient GetImageClient()
    {
        if (_imageClient != null)
        {
            return _imageClient;
        }

        _settings.ValidateImage();

        var dalleClient = new AzureOpenAIClient(
            new Uri(_settings.DalleEndpoint),
            new AzureKeyCredential(_settings.DalleApiKey)
        );

        _imageClient = dalleClient.GetImageClient(_settings.DalleDeploymentId);
        return _imageClient;
    }
}
