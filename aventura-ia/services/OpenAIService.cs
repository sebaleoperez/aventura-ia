using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using OpenAI.Images;
using System.Text.Json;

public class OpenAIService : IAiAdventureService {
    readonly Settings _settings;
    readonly AzureOpenAIClient _client;
    readonly ChatClient _chatClient;
    ImageClient? _imageClient;
    readonly List<ChatMessage> _messages = new List<ChatMessage>();

    public OpenAIService() {
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
            
            // Implementación real de Sora usando HttpClient para llamadas REST API
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_settings.VideoRequestTimeoutSeconds)
            };
            
            // Configurar headers de autenticación para Azure OpenAI Sora
            httpClient.DefaultRequestHeaders.Add("api-key", _settings.SoraApiKey);
            
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
            var response = await httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode) {
                string responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                ConsoleHelper.PrintMessage($"✅ Job de video enviado exitosamente!");
                ConsoleHelper.PrintMessage($"📋 Respuesta completa: {responseContent}");
                
                // Extraer el job ID de la respuesta
                if (jsonResponse.TryGetProperty("id", out var jobIdProperty)) {
                    string jobId = jobIdProperty.GetString() ?? "";
                    ConsoleHelper.PrintMessage($"🎬 Job ID: {jobId}");
                    ConsoleHelper.PrintMessage($"⏳ El video se está procesando. Consulta el estado del job para obtener el resultado.");
                    
                    // Aquí podrías implementar polling para verificar el estado del job
                    // Por ahora, devolvemos el job ID
                    return jobId;
                }
                
                ConsoleHelper.PrintMessage($"⚠️ Job creado pero no se pudo extraer el ID");
                return responseContent;
            } else {
                string errorContent = await response.Content.ReadAsStringAsync();
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
