using Azure;
using Azure.AI.OpenAI;

public class OpenAIService {
    OpenAIClient _client;
    ChatCompletionsOptions _options;
    public OpenAIService() {
        #pragma warning disable CS8604 // Possible null reference argument.
            _client = new OpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")),
                new AzureKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            );
            // Send the first message to the API
            _options = new ChatCompletionsOptions()
            {
                MaxTokens = int.Parse(Environment.GetEnvironmentVariable("MAX_TOKENS")),
                Temperature = float.Parse(Environment.GetEnvironmentVariable("TEMPERATURE")),
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0, 
                DeploymentName = Environment.GetEnvironmentVariable("DEPLOYMENT_SLOT_NAME")
            };
        #pragma warning restore CS8604 // Possible null reference argument.
    }

    public async Task<Uri> GetImageGenerationsAsync(string? scenario, string? graphics) {
        // Get image generation for the first part of the game
        Response<ImageGenerations> imageGenerations = await _client.GetImageGenerationsAsync(
            new ImageGenerationOptions()
            {
                Prompt = "Give me an image of: " + scenario + ". I want the image to follow this type: " + graphics,
                Size = ImageSize.Size1024x1024,
                DeploymentName = Environment.GetEnvironmentVariable("DALLE_SLOT_NAME"),
            });

        // Image Generations responses provide URLs you can use to retrieve requested images
        return imageGenerations.Value.Data[0].Url;
    }

    public async Task<string> GetChatCompletionsAsync(string? input) {
        _options.Messages.Add(new ChatRequestUserMessage(input));
        Response<ChatCompletions> responseWithoutStream = await _client.GetChatCompletionsAsync(_options);
        ChatCompletions response = responseWithoutStream.Value;
        string responseText = response.Choices[0].Message.Content;
        _options.Messages.Add(new ChatRequestAssistantMessage(responseText));
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