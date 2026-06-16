namespace aventura_ia.Tests;

public class GameHelperTests
{
    [Fact]
    public void ParseTranslationsResponse_ParsesJsonResponse()
    {
        const string response = """
        {
          "welcome": "Bienvenido",
          "loading": "Cargando...",
          "scenario": "Escenario:",
          "rounds": "Rondas:",
          "choices": "Opciones:",
          "hints": "Pistas:",
          "difficulty": "Dificultad:",
          "graphics": "Graficos:",
          "imageError": "No se pudo generar la imagen.",
          "hintMessage": "Quieres una pista?",
          "noHints": "No quedan pistas."
        }
        """;

        Translations result = GameHelper.ParseTranslationsResponse(response);

        Assert.Equal("Bienvenido", result.Welcome);
        Assert.Equal("Cargando...", result.Loading);
        Assert.Equal("No quedan pistas.", result.NoHints);
    }

    [Fact]
    public void ParseTranslationsResponse_FallsBackToLegacyLineFormat()
    {
        const string response = """
        Welcome
        Loading
        Scenario
        Rounds
        Choices
        Hints
        Difficulty
        Graphics
        Image error
        Hint message
        No hints
        """;

        Translations result = GameHelper.ParseTranslationsResponse(response);

        Assert.Equal("Welcome", result.Welcome);
        Assert.Equal("Hint message", result.HintMessage);
        Assert.Equal("No hints", result.NoHints);
    }

    [Fact]
    public void ParseRoundResponse_ParsesJsonResponse()
    {
        const string response = """
        {
          "story": "You enter the cave.\n1. Light a torch\n2. Run away",
          "hint": "The dark path needs light.",
          "isGameOver": false
        }
        """;

        GameRoundResponse result = GameHelper.ParseRoundResponse(response);

        Assert.Contains("You enter the cave", result.Story);
        Assert.Equal("The dark path needs light.", result.Hint);
        Assert.False(result.IsGameOver);
    }

    [Fact]
    public void ParseRoundResponse_FallsBackToLegacyHintFormat()
    {
        const string response = "You fell into a trap. Game Over***Choose safer ground.";

        GameRoundResponse result = GameHelper.ParseRoundResponse(response);

        Assert.Equal("You fell into a trap. Game Over", result.Story);
        Assert.Equal("Choose safer ground.", result.Hint);
        Assert.True(result.IsGameOver);
    }
}
