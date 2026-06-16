using System.Text.Json;

public class Translations {
    public required string Welcome { get; set; }
    public required string Loading { get; set; }
    public required string Scenario { get; set; }
    public required string Rounds { get; set; }
    public required string Choices { get; set; }
    public required string Hints { get; set; }
    public required string Difficulty { get; set; }
    public required string Graphics { get; set; }
    public required string ImageError { get; set; }
    public required string HintMessage { get; set; }
    public required string NoHints { get; set; }

}

public sealed class GameRoundResponse
{
    public string Story { get; init; } = string.Empty;
    public string Hint { get; init; } = string.Empty;
    public bool IsGameOver { get; init; }
}

public static class GameHelper {
    public static string GetTranslationsInput(string language) {
        return @$"Translate the UI text values to {language}.
        Always respond only with a valid JSON object. Do not include markdown, code fences,
        comments, or extra text outside the JSON. Use this exact shape and keep the same keys:
        {{
          ""welcome"": ""Welcome to the election game!"",
          ""loading"": ""Loading..."",
          ""scenario"": ""Introduce the scenario in which you want the game to take place:"",
          ""rounds"": ""Introduce the number of rounds:"",
          ""choices"": ""Introduce the number of choices per round:"",
          ""hints"": ""How many available hints ?"",
          ""difficulty"": ""Introduce the desired difficulty (easy, medium, hard, etc):"",
          ""graphics"": ""Choose the desired type of graphics (illustration, realistic, 8 bit, etc):"",
          ""imageError"": ""The required image could not be generated."",
          ""hintMessage"": ""Do you want a hint? If so, type 4. If not, type the option you want to choose:"",
          ""noHints"": ""No hints remaining. Write the option you want to choose:""
        }}";
    }

    public static Translations ParseTranslationsResponse(string response) {
        try
        {
            var translations = JsonSerializer.Deserialize<Translations>(
                response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (translations != null && HasAllTranslations(translations))
            {
                return translations;
            }
        }
        catch (JsonException)
        {
        }

        string[] lines = response
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 11)
        {
            throw new InvalidOperationException("The translation response did not include all required values.");
        }

        return new Translations {
            Welcome = lines[0],
            Loading = lines[1],
            Scenario = lines[2],
            Rounds = lines[3],
            Choices = lines[4],
            Hints = lines[5],
            Difficulty = lines[6],
            Graphics = lines[7],
            ImageError = lines[8],
            HintMessage = lines[9],
            NoHints = lines[10]
        };
    }

    public static string GetGameDescription(Game game) {
        return $@"Create the entire game in {game.Language}, with {game.Rounds} 
        rounds where each round consists of making a choice. Choose correctly to 
        advance to the next round; choose incorrectly, and you lose. 
        The game is set in a scenario described as {game.Scenario}. 
        Each round presents {game.Choices} options: one correct choice that allows 
        me to advance (or win if it's the final round), and the others are incorrect, 
        leading to a loss. The difficulty of distinguishing the correct answer should 
        be {game.Difficulty}. In your next response, introduce the scenario and 
        describe the situation. After two line breaks, list the {game.Choices} 
        options. I will then choose an option, and you will tell me if I won or lost.

        Always respond only with a valid JSON object. Do not include markdown, code fences,
        comments, or extra text outside the JSON. Use this exact shape:
        {{
          ""story"": ""Narrative text and the available options for the player."",
          ""hint"": ""A strong hint to help avoid a wrong choice."",
          ""isGameOver"": false
        }}

        Set isGameOver to true only when my choice is incorrect and I lose.";
    }

    public static GameRoundResponse ParseRoundResponse(string response) {
        try
        {
            var parsedResponse = JsonSerializer.Deserialize<GameRoundResponse>(
                response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (parsedResponse != null && !string.IsNullOrWhiteSpace(parsedResponse.Story))
            {
                return parsedResponse;
            }
        }
        catch (JsonException)
        {
        }

        return new GameRoundResponse
        {
            Story = getRound(response),
            Hint = getHint(response),
            IsGameOver = YouLose(response)
        };
    }

    private static bool YouLose(string response) {
        return response.Contains("Game Over", StringComparison.OrdinalIgnoreCase);
    }

    private static string getHint(string response) {
        if (response.Contains("***")) {
            return response.Split("***")[1];
        }
        return string.Empty;
    }

    private static string getRound(string response) {
        if (response.Contains("***")) {
            return response.Split("***")[0];
        }
        return response;
    }

    private static bool HasAllTranslations(Translations translations)
    {
        return !string.IsNullOrWhiteSpace(translations.Welcome)
            && !string.IsNullOrWhiteSpace(translations.Loading)
            && !string.IsNullOrWhiteSpace(translations.Scenario)
            && !string.IsNullOrWhiteSpace(translations.Rounds)
            && !string.IsNullOrWhiteSpace(translations.Choices)
            && !string.IsNullOrWhiteSpace(translations.Hints)
            && !string.IsNullOrWhiteSpace(translations.Difficulty)
            && !string.IsNullOrWhiteSpace(translations.Graphics)
            && !string.IsNullOrWhiteSpace(translations.ImageError)
            && !string.IsNullOrWhiteSpace(translations.HintMessage)
            && !string.IsNullOrWhiteSpace(translations.NoHints);
    }
}
