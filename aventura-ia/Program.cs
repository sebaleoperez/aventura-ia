using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Cargar configuración de menús
var menuConfig = MenuConfig.LoadFromFile("config/menu-options.json");

static ushort SelectRounds(string prompt, MenuConfig config)
{
    return ConsoleHelper.SelectFromMenuWithCustom<ushort>(
        prompt, 
        config.Rounds, 
        "Ingresa el número de rondas personalizado (1-50):",
        input => {
            if (ushort.TryParse(input, out ushort result) && result > 0 && result <= 50)
                return result;
            ConsoleHelper.PrintMessage("Valor inválido. Usando 5 rondas por defecto.");
            return 5;
        }
    );
}

static ushort SelectChoices(string prompt, MenuConfig config)
{
    return ConsoleHelper.SelectFromMenuWithCustom<ushort>(
        prompt, 
        config.Choices, 
        "Ingresa el número de opciones por ronda (2-10):",
        input => {
            if (ushort.TryParse(input, out ushort result) && result >= 2 && result <= 10)
                return result;
            ConsoleHelper.PrintMessage("Valor inválido. Usando 3 opciones por defecto.");
            return 3;
        }
    );
}

static ushort SelectHints(string prompt, MenuConfig config)
{
    return ConsoleHelper.SelectFromMenuWithCustom<ushort>(
        prompt, 
        config.Hints, 
        "Ingresa el número de pistas disponibles (0-20):",
        input => {
            if (ushort.TryParse(input, out ushort result) && result <= 20)
                return result;
            ConsoleHelper.PrintMessage("Valor inválido. Usando 2 pistas por defecto.");
            return 2;
        }
    );
}

static string SelectDifficulty(string prompt, MenuConfig config)
{
    return ConsoleHelper.SelectFromMenuWithCustom<string>(
        prompt, 
        config.Difficulty, 
        "Describe la dificultad personalizada (ej: muy fácil, imposible, etc.):",
        input => string.IsNullOrWhiteSpace(input) ? "medium" : input.Trim()
    );
}

static string SelectGraphics(string prompt, MenuConfig config)
{
    return ConsoleHelper.SelectFromMenuWithCustom<string>(
        prompt, 
        config.Graphics, 
        "Describe el estilo de gráficos personalizado (ej: cyberpunk, medieval, etc.):",
        input => string.IsNullOrWhiteSpace(input) ? "illustration" : input.Trim()
    );
}

// Set up DI
var serviceProvider = new ServiceCollection()
    .AddSingleton<OpenAIService>()
    .BuildServiceProvider();

// Retrieve an instance of OpenAIService from the DI container
var openAIService = serviceProvider.GetService<OpenAIService>();

// Language to use for the game
string? language;
bool generateVideo = false;

// Check if any arguments were passed
if (args.Length > 0)
{
    // Check for --video parameter
    if (args.Contains("--video"))
    {
        generateVideo = true;
        ConsoleHelper.PrintMessage("🎬 Modo de generación de video activado!");
    }
    
    // Get language (first non-flag argument)
    language = args.FirstOrDefault(arg => !arg.StartsWith("--"));
    
    if (!string.IsNullOrEmpty(language))
    {
        // Print the language
        ConsoleHelper.PrintMessage($"Selected Language: {language}");
    }
    else
    {
        // Ask the user to input the language
        language = ConsoleHelper.ReadData("Insert language:");
    }
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
    Rounds = SelectRounds(translations.Rounds, menuConfig),
    Choices = SelectChoices(translations.Choices, menuConfig),
    Hints = SelectHints(translations.Hints, menuConfig),
    Difficulty = SelectDifficulty(translations.Difficulty, menuConfig),
    Graphics = SelectGraphics(translations.Graphics, menuConfig),
};

currentGame.RemainingHints = currentGame.Hints;

ConsoleHelper.PrintMessage(translations.Loading);

// Generar video si se especificó el parámetro --video
if (generateVideo && !string.IsNullOrEmpty(currentGame.Scenario))
{
    try 
    {
        string videoUrl = await openAIService.GenerateVideoAsync(currentGame.Scenario);
        if (!string.IsNullOrEmpty(videoUrl))
        {
            ConsoleHelper.PrintMessage($"🎥 Video del escenario disponible en: {videoUrl}");
        }
    }
    catch (Exception ex)
    {
        ConsoleHelper.PrintMessage($"❌ Error al generar video: {ex.Message}");
    }
}

try 
{
    currentGame.IntroductionImage = await openAIService.GetImageGenerationsAsync(currentGame.Scenario, currentGame.Graphics);
    ConsoleHelper.OpenImage(currentGame.IntroductionImage);
}
catch {
    ConsoleHelper.PrintMessage(translations.ImageError);
}

string response = await openAIService.GetChatCompletionsAsync(GameHelper.GetGameDescription(currentGame));
currentGame.Introduction = GameHelper.getRound(response);
string currentHint = GameHelper.getHint(response);

ConsoleHelper.PrintMessage(currentGame.Introduction);

while (!currentGame.GameOver)
{
    RoundDetails currentRound = new RoundDetails();
    currentRound.Hint = currentHint;

    if (currentGame.RemainingHints > 0)
    {
        string? hintChoice = ConsoleHelper.ReadData(translations.HintMessage);
        if (hintChoice == "4")
        {
            ConsoleHelper.PrintMessage(currentHint);
            currentGame.RemainingHints--;

            // Get the user's choice
            currentRound.Choice = ConsoleHelper.ReadData(string.Empty);
        }
        else {
            currentRound.Choice = hintChoice;
        }
    }
    else {
        ConsoleHelper.PrintMessage(translations.NoHints);

        // Get the user's choice
        currentRound.Choice = ConsoleHelper.ReadData(string.Empty);
    }

    ConsoleHelper.Clear();
    ConsoleHelper.PrintMessage(translations.Loading);

    response = await openAIService.GetChatCompletionsAsync(currentRound.Choice);

    currentRound.Response = GameHelper.getRound(response);
    currentHint = GameHelper.getHint(response);
    
    currentGame.GameOver = GameHelper.YouLose(currentRound.Response);

    try
    {
        currentRound.Image = await openAIService.GetImageGenerationsAsync(currentRound.Response, currentGame.Graphics);
        ConsoleHelper.OpenImage(currentRound.Image);
    }
    catch {
        ConsoleHelper.PrintMessage(translations.ImageError);
    }

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