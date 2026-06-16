namespace aventura_ia.Tests;

public class GameEngineTests
{
    [Fact]
    public void Start_StoresIntroductionAndInitializesRemainingHints()
    {
        var game = new Game { Hints = 2 };
        var introduction = new GameRoundResponse { Story = "Intro", Hint = "Hint" };

        GameEngine.Start(game, introduction);

        Assert.Equal("Intro", game.Introduction);
        Assert.Equal<uint>(2, game.RemainingHints);
    }

    [Fact]
    public void TryUseHint_DecrementsRemainingHintsUntilEmpty()
    {
        var game = new Game { RemainingHints = 1 };

        bool firstUse = GameEngine.TryUseHint(game);
        bool secondUse = GameEngine.TryUseHint(game);

        Assert.True(firstUse);
        Assert.False(secondUse);
        Assert.Equal<uint>(0, game.RemainingHints);
    }

    [Fact]
    public void CompleteRound_EndsGameWhenRoundLimitIsReached()
    {
        var game = new Game { Rounds = 1 };
        RoundDetails round = GameEngine.CreateRound("1", "Hint");

        GameEngine.ApplyRoundResponse(game, round, new GameRoundResponse { Story = "Survived" });
        GameEngine.CompleteRound(game, round);

        Assert.True(game.GameOver);
        Assert.Equal<uint>(1, game.CurrentRound);
        Assert.Single(game.RoundDetails);
    }
}
