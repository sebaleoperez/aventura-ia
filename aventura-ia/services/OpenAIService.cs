using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
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

    public OpenAIService(HttpClient httpClient) {
        _httpClient = httpClient;

        IConfigurationRoot config = BuildConfiguration();
        _settings = config.GetSection("Settings").Get<Settings>() ?? new Settings();
        _settings.ValidateChat();

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
        try {
            _settings.ValidateVideo();
            
            // Crear prompt optimizado para Sora
            string videoPrompt = $"Create a cinematic video scene of: {scenario}. The scene should be atmospheric, detailed, and immersive, showing the environment and setting in a way that brings the adventure scenario to life.";
            
            ConsoleHelper.PrintMessage($"🎬 Generando video con Sora para el escenario: {scenario}");
            ConsoleHelper.PrintMessage($"📝 Prompt del video: {videoPrompt}");
            
            // Crear el payload para la API de Sora según el formato de Azure OpenAI
            var requestBody = new {
                model = _settings.SoraDeploymentId,
                prompt = videoPrompt,
                height = "1080",
                width = "1920", // Formato widescreen 16:9
                n_seconds = "10", // 10 segundos de duración
                n_variants = "1" // Una variante
            };
            
            string jsonContent = JsonSerializer.Serialize(requestBody);
            // El Content-Type se configura automáticamente en StringContent
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            // Construir la URL del endpoint para Sora
            string apiUrl = $"{_settings.SoraEndpoint.TrimEnd('/')}/openai/v1/video/generations/jobs?api-version=preview";
            
            ConsoleHelper.PrintMessage("Enviando solicitud a Sora...");

            // Enviar la solicitud
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.VideoRequestTimeoutSeconds));
            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = content
            };
            request.Headers.Add("api-key", _settings.SoraApiKey);

            using HttpResponseMessage response = await _httpClient.SendAsync(request, timeout.Token);
            
            if (response.IsSuccessStatusCode) {
                string responseContent = await response.Content.ReadAsStringAsync(timeout.Token);
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                ConsoleHelper.PrintMessage($"✅ Job de video enviado exitosamente!");
                ConsoleHelper.PrintMessage($"📋 Respuesta completa: {responseContent}");
                
                // Extraer el job ID de la respuesta
                if (jsonResponse.TryGetProperty("id", out var jobIdProperty)) {
                    string jobId = jobIdProperty.GetString() ?? "";
                    ConsoleHelper.PrintMessage($"🎬 Job ID: {jobId}");

                    if (!_settings.EnableVideoPolling)
                    {
                        ConsoleHelper.PrintMessage($"⏳ El video se está procesando. Consulta el estado del job para obtener el resultado.");
                        return jobId;
                    }

                    return await WaitForVideoJobAsync(jobId);
                }
                
                ConsoleHelper.PrintMessage($"⚠️ Job creado pero no se pudo extraer el ID");
                return responseContent;
            } else {
                string errorContent = await response.Content.ReadAsStringAsync(timeout.Token);
                ConsoleHelper.PrintMessage($"❌ Error en la API de Sora: {response.StatusCode}");
                ConsoleHelper.PrintMessage($"Detalles: {errorContent}");
                return string.Empty;
            }
        }
        catch (HttpRequestException httpEx) {
            ConsoleHelper.PrintMessage($"❌ Error de conexión con Sora: {httpEx.Message}");
            ConsoleHelper.PrintMessage($"💡 Verifica que tu endpoint y clave API de Sora sean correctos");
            return string.Empty;
        }
        catch (TaskCanceledException timeoutEx) {
            ConsoleHelper.PrintMessage($"❌ La solicitud a Sora excedió el tiempo de espera: {timeoutEx.Message}");
            return string.Empty;
        }
        catch (JsonException jsonEx) {
            ConsoleHelper.PrintMessage($"❌ Error al procesar respuesta JSON: {jsonEx.Message}");
            return string.Empty;
        }
        catch (Exception ex) {
            ConsoleHelper.PrintMessage($"❌ Error generando video: {ex.Message}");
            return string.Empty;
        }
    }

    private async Task<string> WaitForVideoJobAsync(string jobId)
    {
        string statusUrl = $"{_settings.SoraEndpoint.TrimEnd('/')}/openai/v1/video/generations/jobs/{jobId}?api-version=preview";
        TimeSpan pollingInterval = TimeSpan.FromSeconds(_settings.VideoPollingIntervalSeconds);
        TimeSpan pollingTimeout = TimeSpan.FromSeconds(_settings.VideoPollingTimeoutSeconds);
        Stopwatch stopwatch = Stopwatch.StartNew();

        ConsoleHelper.PrintMessage($"Esperando resultado del video cada {_settings.VideoPollingIntervalSeconds} segundos...");

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
                ConsoleHelper.PrintMessage($"No se pudo consultar el estado del video: {response.StatusCode}");
                ConsoleHelper.PrintMessage($"Detalles: {responseContent}");
                return jobId;
            }

            JsonElement jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            string status = GetVideoJobStatus(jsonResponse);
            ConsoleHelper.PrintMessage($"Estado del video: {status}");

            if (IsCompletedVideoStatus(status))
            {
                string? videoUrl = FindFirstStringProperty(jsonResponse, "url", "uri", "video_url", "download_url");
                return string.IsNullOrWhiteSpace(videoUrl) ? responseContent : videoUrl;
            }

            if (IsFailedVideoStatus(status))
            {
                ConsoleHelper.PrintMessage($"El job de video terminó sin éxito: {responseContent}");
                return string.Empty;
            }
        }

        ConsoleHelper.PrintMessage($"El video no terminó dentro de {_settings.VideoPollingTimeoutSeconds} segundos. Job ID: {jobId}");
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
