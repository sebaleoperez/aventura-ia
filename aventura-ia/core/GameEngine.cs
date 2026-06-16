public static class GameEngine
{
    public static void Start(Game game, GameRoundResponse introduction)
    {
        game.Introduction = introduction.Story;
        game.RemainingHints = game.Hints;
    }

    public static bool TryUseHint(Game game)
    {
        if (game.RemainingHints == 0)
        {
            return false;
        }

        game.RemainingHints--;
        return true;
    }

    public static RoundDetails CreateRound(string? choice, string hint)
    {
        return new RoundDetails
        {
            Choice = choice,
            Hint = hint
        };
    }

    public static void ApplyRoundResponse(Game game, RoundDetails round, GameRoundResponse response)
    {
        round.Response = response.Story;
        game.GameOver = response.IsGameOver;
    }

    public static void CompleteRound(Game game, RoundDetails round)
    {
        game.RoundDetails.Add(round);

        if (game.GameOver)
        {
            return;
        }

        game.CurrentRound++;
        game.GameOver = game.CurrentRound >= game.Rounds;
    }
}
