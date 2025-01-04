using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using OpenAI.Images;

public class OpenAIService {
    AzureOpenAIClient _client;
    AzureOpenAIClient _dalleClient;
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
}