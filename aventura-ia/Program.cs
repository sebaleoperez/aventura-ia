using Microsoft.Extensions.DependencyInjection;

MenuConfig menuConfig = MenuConfig.LoadFromFile("config/menu-options.json");
CommandLineOptions options = CommandLineOptions.Parse(args);

var serviceProvider = new ServiceCollection()
    .AddSingleton(ConfigurationHelper.LoadSettings())
    .AddSingleton<HttpClient>()
    .AddSingleton<IAiAdventureService, OpenAIService>()
    .BuildServiceProvider();

var aiAdventureService = serviceProvider.GetRequiredService<IAiAdventureService>();
var gameRunner = new ConsoleGameRunner(aiAdventureService, menuConfig, options);

await gameRunner.RunAsync();
