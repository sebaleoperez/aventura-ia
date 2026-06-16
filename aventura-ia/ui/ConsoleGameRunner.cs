public sealed class ConsoleGameRunner
{
    private readonly IAiAdventureService _aiAdventureService;
    private readonly MenuConfig _menuConfig;
    private readonly CommandLineOptions _options;

    public ConsoleGameRunner(
        IAiAdventureService aiAdventureService,
        MenuConfig menuConfig,
        CommandLineOptions options)
    {
        _aiAdventureService = aiAdventureService;
        _menuConfig = menuConfig;
        _options = options;
    }

    public async Task RunAsync()
    {
        PrintSelectedModes();

        string? language = GetLanguage();
        Translations translations = await _aiAdventureService.GetTranslationsAsync(language!);

        ConsoleHelper.PrintTitle(translations.Welcome);

        Game currentGame = CreateGame(language, translations);

        ConsoleHelper.PrintMessage(translations.Loading);

        await GenerateInitialVideoAsync(currentGame);
        await GenerateIntroductionImageAsync(currentGame, translations);

        string response = await _aiAdventureService.GetChatCompletionsAsync(GameHelper.GetGameDescription(currentGame));
        GameRoundResponse parsedResponse = GameHelper.ParseRoundResponse(response);
        GameEngine.Start(currentGame, parsedResponse);
        string currentHint = parsedResponse.Hint;

        ConsoleHelper.PrintMessage(currentGame.Introduction);

        while (!currentGame.GameOver)
        {
            string? choice = ReadRoundChoice(currentGame, translations, currentHint);
            RoundDetails currentRound = GameEngine.CreateRound(choice, currentHint);

            ConsoleHelper.Clear();
            ConsoleHelper.PrintMessage(translations.Loading);

            response = await _aiAdventureService.GetChatCompletionsAsync(currentRound.Choice);

            parsedResponse = GameHelper.ParseRoundResponse(response);
            GameEngine.ApplyRoundResponse(currentGame, currentRound, parsedResponse);
            currentHint = parsedResponse.Hint;

            await GenerateRoundImageAsync(currentGame, currentRound, translations);

            ConsoleHelper.Clear();
            if (parsedResponse.IsGameOver)
                ConsoleHelper.PrintColoredMessage(currentRound.Response, ConsoleColor.Red);
            else
                ConsoleHelper.PrintColoredMessage(currentRound.Response, ConsoleColor.Green);

            GameEngine.CompleteRound(currentGame, currentRound);
        }
    }

    private void PrintSelectedModes()
    {
        if (_options.GenerateVideo)
        {
            ConsoleHelper.PrintMessage("🎬 Modo de generación de video activado!");
        }

        if (_options.TextOnly)
        {
            ConsoleHelper.PrintMessage("Modo solo texto activado.");
        }
        else if (!_options.GenerateImages)
        {
            ConsoleHelper.PrintMessage("Modo sin imágenes activado.");
        }
    }

    private string? GetLanguage()
    {
        if (!string.IsNullOrEmpty(_options.Language))
        {
            ConsoleHelper.PrintMessage($"Selected Language: {_options.Language}");
            return _options.Language;
        }

        return ConsoleHelper.ReadData("Insert language:");
    }

    private Game CreateGame(string? language, Translations translations)
    {
        return new Game
        {
            Language = language,
            Scenario = ConsoleHelper.ReadData(translations.Scenario),
            Rounds = SelectRounds(translations.Rounds, _menuConfig),
            Choices = SelectChoices(translations.Choices, _menuConfig),
            Hints = SelectHints(translations.Hints, _menuConfig),
            Difficulty = SelectDifficulty(translations.Difficulty, _menuConfig),
            Graphics = _options.GenerateImages ? SelectGraphics(translations.Graphics, _menuConfig) : "text-only",
        };
    }

    private async Task GenerateInitialVideoAsync(Game currentGame)
    {
        if (!_options.GenerateVideo || string.IsNullOrEmpty(currentGame.Scenario))
        {
            return;
        }

        try
        {
            ConsoleHelper.PrintMessage($"🎬 Generando video con Sora para el escenario: {currentGame.Scenario}");
            string videoUrl = await _aiAdventureService.GenerateVideoAsync(currentGame.Scenario);
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

    private async Task GenerateIntroductionImageAsync(Game currentGame, Translations translations)
    {
        if (!_options.GenerateImages)
        {
            return;
        }

        try
        {
            currentGame.IntroductionImage = await _aiAdventureService.GetImageGenerationsAsync(currentGame.Scenario, currentGame.Graphics);
            ConsoleHelper.OpenImage(currentGame.IntroductionImage);
        }
        catch
        {
            ConsoleHelper.PrintMessage(translations.ImageError);
        }
    }

    private string? ReadRoundChoice(Game currentGame, Translations translations, string currentHint)
    {
        if (currentGame.RemainingHints > 0)
        {
            string? hintChoice = ConsoleHelper.ReadData(translations.HintMessage);
            if (hintChoice == "4")
            {
                ConsoleHelper.PrintMessage(currentHint);
                GameEngine.TryUseHint(currentGame);
                return ConsoleHelper.ReadData(string.Empty);
            }

            return hintChoice;
        }

        ConsoleHelper.PrintMessage(translations.NoHints);
        return ConsoleHelper.ReadData(string.Empty);
    }

    private async Task GenerateRoundImageAsync(Game currentGame, RoundDetails currentRound, Translations translations)
    {
        if (!_options.GenerateImages)
        {
            return;
        }

        try
        {
            currentRound.Image = await _aiAdventureService.GetImageGenerationsAsync(currentRound.Response, currentGame.Graphics);
            ConsoleHelper.OpenImage(currentRound.Image);
        }
        catch
        {
            ConsoleHelper.PrintMessage(translations.ImageError);
        }
    }

    private static ushort SelectRounds(string prompt, MenuConfig config)
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

    private static ushort SelectChoices(string prompt, MenuConfig config)
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

    private static ushort SelectHints(string prompt, MenuConfig config)
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

    private static string SelectDifficulty(string prompt, MenuConfig config)
    {
        return ConsoleHelper.SelectFromMenuWithCustom<string>(
            prompt,
            config.Difficulty,
            "Describe la dificultad personalizada (ej: muy fácil, imposible, etc.):",
            input => string.IsNullOrWhiteSpace(input) ? "medium" : input.Trim()
        );
    }

    private static string SelectGraphics(string prompt, MenuConfig config)
    {
        return ConsoleHelper.SelectFromMenuWithCustom<string>(
            prompt,
            config.Graphics,
            "Describe el estilo de gráficos personalizado (ej: cyberpunk, medieval, etc.):",
            input => string.IsNullOrWhiteSpace(input) ? "illustration" : input.Trim()
        );
    }
}
