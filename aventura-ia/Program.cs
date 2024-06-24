using Microsoft.Extensions.DependencyInjection;

// Set up DI
var serviceProvider = new ServiceCollection()
    .AddSingleton<OpenAIService>()
    .BuildServiceProvider();

// Retrieve an instance of OpenAIService from the DI container
var openAIService = serviceProvider.GetService<OpenAIService>();

// Language to use for the game
string? language;

// Check if any arguments were passed
if (args.Length > 0)
{
    // Access the first argument
    language = args[0];

    // Print the language
    ConsoleHelper.PrintMessage($"Selected Language: {language}");
}
else
{
    // Ask the user to input the language
    language = ConsoleHelper.ReadData("Insert language:");
}

// Get translations for the game
Translations translations = await openAIService!.GetTranslationsAsync(language!);

ConsoleHelper.PrintTitle(translations.Welcome);

Game currentGame = new Game
{
    Language = language,
    Scenario = ConsoleHelper.ReadData(translations.Scenario),
    Rounds = Convert.ToUInt16(ConsoleHelper.ReadData(translations.Rounds)),
    Choices = Convert.ToUInt16(ConsoleHelper.ReadData(translations.Choices)),
    Difficulty = ConsoleHelper.ReadData(translations.Difficulty),
    Graphics = ConsoleHelper.ReadData(translations.Graphics),
};

ConsoleHelper.PrintMessage(translations.Loading);
currentGame.IntroductionImage = await openAIService.GetImageGenerationsAsync(currentGame.Scenario, currentGame.Graphics);
currentGame.Introduction = await openAIService.GetChatCompletionsAsync(GameHelper.GetGameDescription(currentGame));

ConsoleHelper.OpenImage(currentGame.IntroductionImage);
ConsoleHelper.PrintMessage(currentGame.Introduction);

while (!currentGame.GameOver)
{
    RoundDetails currentRound = new RoundDetails();
    // Get the user's choice
    currentRound.Choice = ConsoleHelper.ReadData(string.Empty);

    ConsoleHelper.Clear();
    ConsoleHelper.PrintMessage(translations.Loading);

    currentRound.Response = await openAIService.GetChatCompletionsAsync(currentRound.Choice);
    
    currentGame.GameOver = GameHelper.YouLose(currentRound.Response);

    currentRound.Image = await openAIService.GetImageGenerationsAsync(currentRound.Response, currentGame.Graphics);
    ConsoleHelper.OpenImage(currentRound.Image);

    ConsoleHelper.Clear();
    if (GameHelper.YouLose(currentRound.Response)) 
        ConsoleHelper.PrintColoredMessage(currentRound.Response, ConsoleColor.Red);
    else
        ConsoleHelper.PrintColoredMessage(currentRound.Response, ConsoleColor.Green);

    currentGame.RoundDetails.Add(currentRound);
    if (!currentGame.GameOver)
    {
        currentGame.CurrentRound++;
        currentGame.GameOver = currentGame.CurrentRound >= currentGame.Rounds;
    }
}