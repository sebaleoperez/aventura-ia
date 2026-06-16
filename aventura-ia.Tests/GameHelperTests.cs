namespace aventura_ia.Tests;

public class GameHelperTests
{
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
