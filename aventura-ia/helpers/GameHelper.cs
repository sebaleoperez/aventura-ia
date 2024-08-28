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

public static class GameHelper {
    public static string GetTranslationsInput(string language) {
        return @$"I want you to translate the following sentences in {language}, I don't want anything else in the response but the translations.
        Welcome to the election game!
        Loading...
        Introduce the scenario in which you want the game to take place:
        Introduce the number of rounds:
        Introduce the number of choices per round:
        How many available hints ?
        Introduce the desired difficulty (easy, medium, hard, etc):
        Choose the desired type of graphics (illustration, realistic, 8 bit, etc):
        The required image could not be generated.
        Do you want a hint? If so, type 4. If not, type the option you want to choose:
        No hints remaining. Write the option you want to choose:";
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
        If I win, conclude the story. If I lose, include the phrase 'Game Over' 
        in your response. At the end of each response, give a strong hint to help me 
        avoid a wrong choice, woven naturally into the story. This hint should begin 
        with *** so I can easily identify it.";
    }

    public static bool YouLose(string response) {
        return response.Contains("Game Over");
    }

    public static string getHint(string response) {
        if (response.Contains("***")) {
            return response.Split("***")[1];
        }
        return string.Empty;
    }

    public static string getRound(string response) {
        if (response.Contains("***")) {
            return response.Split("***")[0];
        }
        return response;
    }
}