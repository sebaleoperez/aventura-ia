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
    .AddSingleton<HttpClient>()
    .AddSingleton<IAiAdventureService, OpenAIService>()
    .BuildServiceProvider();

// Retrieve an instance of IAiAdventureService from the DI container
var aiAdventureService = serviceProvider.GetRequiredService<IAiAdventureService>();

CommandLineOptions options = CommandLineOptions.Parse(args);

if (options.GenerateVideo)
{
    ConsoleHelper.PrintMessage("🎬 Modo de generación de video activado!");
}

if (options.TextOnly)
{
    ConsoleHelper.PrintMessage("Modo solo texto activado.");
}
else if (!options.GenerateImages)
{
    ConsoleHelper.PrintMessage("Modo sin imágenes activado.");
}

// Language to use for the game
string? language = options.Language;

if (!string.IsNullOrEmpty(language))
{
    ConsoleHelper.PrintMessage($"Selected Language: {language}");
}
else
{
    language = ConsoleHelper.ReadData("Insert language:");
}

// Get translations for the game
Translations translations = await aiAdventureService.GetTranslationsAsync(language!);

ConsoleHelper.PrintTitle(translations.Welcome);

Game currentGame = new Game
{
    Language = language,
    Scenario = ConsoleHelper.ReadData(translations.Scenario),
    Rounds = SelectRounds(translations.Rounds, menuConfig),
    Choices = SelectChoices(translations.Choices, menuConfig),
    Hints = SelectHints(translations.Hints, menuConfig),
    Difficulty = SelectDifficulty(translations.Difficulty, menuConfig),
    Graphics = options.GenerateImages ? SelectGraphics(translations.Graphics, menuConfig) : "text-only",
};

ConsoleHelper.PrintMessage(translations.Loading);

// Generar video si se especificó el parámetro --video
if (options.GenerateVideo && !string.IsNullOrEmpty(currentGame.Scenario))
{
    try 
    {
        string videoUrl = await aiAdventureService.GenerateVideoAsync(currentGame.Scenario);
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

if (options.GenerateImages)
{
    try
    {
        currentGame.IntroductionImage = await aiAdventureService.GetImageGenerationsAsync(currentGame.Scenario, currentGame.Graphics);
        ConsoleHelper.OpenImage(currentGame.IntroductionImage);
    }
    catch {
        ConsoleHelper.PrintMessage(translations.ImageError);
    }
}
string response = await aiAdventureService.GetChatCompletionsAsync(GameHelper.GetGameDescription(currentGame));
GameRoundResponse parsedResponse = GameHelper.ParseRoundResponse(response);
GameEngine.Start(currentGame, parsedResponse);
string currentHint = parsedResponse.Hint;

ConsoleHelper.PrintMessage(currentGame.Introduction);

while (!currentGame.GameOver)
{
    string? choice;

    if (currentGame.RemainingHints > 0)
    {
        string? hintChoice = ConsoleHelper.ReadData(translations.HintMessage);
        if (hintChoice == "4")
        {
            ConsoleHelper.PrintMessage(currentHint);
            GameEngine.TryUseHint(currentGame);

            // Get the user's choice
            choice = ConsoleHelper.ReadData(string.Empty);
        }
        else {
            choice = hintChoice;
        }
    }
    else {
        ConsoleHelper.PrintMessage(translations.NoHints);

        // Get the user's choice
        choice = ConsoleHelper.ReadData(string.Empty);
    }

    RoundDetails currentRound = GameEngine.CreateRound(choice, currentHint);

    ConsoleHelper.Clear();
    ConsoleHelper.PrintMessage(translations.Loading);

    response = await aiAdventureService.GetChatCompletionsAsync(currentRound.Choice);

    parsedResponse = GameHelper.ParseRoundResponse(response);
    GameEngine.ApplyRoundResponse(currentGame, currentRound, parsedResponse);
    currentHint = parsedResponse.Hint;

    if (options.GenerateImages)
    {
        try
        {
            currentRound.Image = await aiAdventureService.GetImageGenerationsAsync(currentRound.Response, currentGame.Graphics);
            ConsoleHelper.OpenImage(currentRound.Image);
        }
        catch {
            ConsoleHelper.PrintMessage(translations.ImageError);
        }
    }

    ConsoleHelper.Clear();
    if (parsedResponse.IsGameOver)
        ConsoleHelper.PrintColoredMessage(currentRound.Response, ConsoleColor.Red);
    else
        ConsoleHelper.PrintColoredMessage(currentRound.Response, ConsoleColor.Green);

    GameEngine.CompleteRound(currentGame, currentRound);
}
