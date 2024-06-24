public class Game
{
    public string? Language { get; init; }
    public string? Scenario { get; init; }
    public UInt16 Rounds { get; init; }
    public UInt16 Choices { get; init; }
    public string? Difficulty { get; init; }
    public string? Graphics { get; init; }
    public string? Introduction { get; set; }
    public Uri? IntroductionImage { get; set; }
    public bool GameOver { get; set; } = false;
    public uint CurrentRound { get; set; } = 0;

    public List<RoundDetails> RoundDetails { get; init; } = new();
}

public class RoundDetails
{
    public string? Choice { get; set; }
    public string? Response { get; set; }
    public Uri? Image { get; set; }
}