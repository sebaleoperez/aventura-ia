public class Translations {
    public required string Welcome { get; set; }
    public required string Loading { get; set; }
    public required string Scenario { get; set; }
    public required string Rounds { get; set; }
    public required string Choices { get; set; }
    public required string Difficulty { get; set; }
    public required string Graphics { get; set; }
    public required string ImageError { get; set; }
}

public static class GameHelper {
    public static string GetTranslationsInput(string language) {
        return @$"I want you to translate the following sentences in {language}, I don't want anything else in the response but the translations.
        Welcome to the election game!
        Loading...
        Introduce the scenario in which you want the game to take place:
        Introduce the number of rounds:
        Introduce the number of choices per round:
        Introduce the desired difficulty (easy, medium, hard, etc):
        Choose the desired type of graphics (illustration, realistic, 8 bit, etc):
        The required image could not be generated.";
    }

    public static string GetGameDescription(Game game) {
        return $@"I want you to represent a game for me with {game.Choices} choices in an adventure style.
        The game consists of passing {game.Rounds} rounds of choices. If you guess the correct choice you pass the round until you reach the last one
        but if you fail you will lose.
        The context is in a scenario described as {game.Scenario}. Each round has {game.Choices} choices, one is correct
        and makes you pass the round (or win if it is the last round) and the rest are incorrect and make you lose. 
        The level of difficulty to distinguish the correct answer should be {game.Difficulty}. 
        In your next response I want you to list the {game.Choices} options,
        then I will respond with the option I choose and you respond if I won or lost. As it is a game, when I win or lose
        I want you to give me a response giving an end to the story. Your message will first make an introduction to the scenario,
        describing the situation and then, separated by two line breaks, you will give me the {game.Choices} options. 
        The whole game must be in the {game.Language} language. When you lose, the response must contain the text 'Game Over'.
        If the answer is correct, it must not contain the text 'Game Over'.";
    }

    public static bool YouLose(string response) {
        return response.Contains("Game Over");
    }
}