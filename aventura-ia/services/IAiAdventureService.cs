public interface IAiAdventureService
{
    Task<Translations> GetTranslationsAsync(string language);
    Task<string> GetChatCompletionsAsync(string? input);
    Task<Uri> GetImageGenerationsAsync(string? scenario, string? graphics);
    Task<string> GenerateVideoAsync(string scenario);
}
