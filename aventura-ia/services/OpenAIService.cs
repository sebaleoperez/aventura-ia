using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using OpenAI.Images;
using System.Text.Json;

public class OpenAIService {
    AzureOpenAIClient _client;
    AzureOpenAIClient _dalleClient;
    AzureOpenAIClient _soraClient;
    ChatClient _chatClient;
    ImageClient _imageClient;
    List<ChatMessage> _messages = new List<ChatMessage>();
    public OpenAIService() {
        IConfigurationRoot config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();
        #pragma warning disable CS8604 // Possible null reference argument.
            _client = new AzureOpenAIClient(
                new Uri(settings?.AzureOpenAiEndpoint),
                new AzureKeyCredential(settings?.AzureOpenAiApiKey)
            );
            _dalleClient = new AzureOpenAIClient(
                new Uri(settings?.DalleEndpoint),
                new AzureKeyCredential(settings?.DalleApiKey)
            );
            _soraClient = new AzureOpenAIClient(
                new Uri(settings?.SoraEndpoint),
                new AzureKeyCredential(settings?.SoraApiKey)
            );

            _chatClient = _client.GetChatClient(settings?.AzureOpenAiDeploymentId);
            _imageClient = _dalleClient.GetImageClient(settings?.DalleDeploymentId);
        #pragma warning restore CS8604 // Possible null reference argument.
    }

    public async Task<Uri> GetImageGenerationsAsync(string? scenario, string? graphics) {
        // Get image generation for the first part of the game
        string prompt = "Give me an image of: " + scenario + ". I want the image to follow this type: " + graphics;
        ImageGenerationOptions options = new()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Vivid,
            ResponseFormat = GeneratedImageFormat.Uri
        };
        GeneratedImage image = await _imageClient.GenerateImageAsync(prompt, options);

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

        List<string> translations = (await GetChatCompletionsAsync(initialInput)).Split("\n").ToList();

        return new Translations {
            Welcome = translations[0],
            Loading = translations[1],
            Scenario = translations[2],
            Rounds = translations[3],
            Choices = translations[4],
            Hints = translations[5],
            Difficulty = translations[6],
            Graphics = translations[7],
            ImageError = translations[8],
            HintMessage = translations[9],
            NoHints = translations[10]
        };
    }

    public async Task<string> GenerateVideoAsync(string scenario) {
        try {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();
            
            // Crear prompt optimizado para Sora
            string videoPrompt = $"Create a cinematic video scene of: {scenario}. The scene should be atmospheric, detailed, and immersive, showing the environment and setting in a way that brings the adventure scenario to life.";
            
            ConsoleHelper.PrintMessage($"🎬 Generando video con Sora para el escenario: {scenario}");
            ConsoleHelper.PrintMessage($"📝 Prompt del video: {videoPrompt}");
            
            // Implementación real de Sora usando HttpClient para llamadas REST API
            using var httpClient = new HttpClient();
            
            // Configurar headers de autenticación para Azure OpenAI Sora
            httpClient.DefaultRequestHeaders.Add("Api-key", settings?.SoraApiKey);
            
            // Crear el payload para la API de Sora según el formato de Azure OpenAI
            var requestBody = new {
                model = settings?.SoraDeploymentId ?? "sora", // Usar "sora" como default
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
            string apiUrl = $"{settings?.SoraEndpoint?.TrimEnd('/')}/openai/v1/video/generations/jobs?api-version=preview";
            
            ConsoleHelper.PrintMessage("� Enviando solicitud a Sora...");
            
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
                ConsoleHelper.PrintMessage($"� Detalles: {errorContent}");
                return string.Empty;
            }
        }
        catch (HttpRequestException httpEx) {
            ConsoleHelper.PrintMessage($"❌ Error de conexión con Sora: {httpEx.Message}");
            ConsoleHelper.PrintMessage($"💡 Verifica que tu endpoint y clave API de Sora sean correctos");
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
}