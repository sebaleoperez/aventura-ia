using System.Text.Json;

public class MenuConfig
{
    public Dictionary<string, object> Rounds { get; set; } = new();
    public Dictionary<string, object> Choices { get; set; } = new();
    public Dictionary<string, object> Hints { get; set; } = new();
    public Dictionary<string, object> Difficulty { get; set; } = new();
    public Dictionary<string, object> Graphics { get; set; } = new();

    public static MenuConfig LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                // Si no existe el archivo, crear uno con valores por defecto
                var defaultConfig = CreateDefaultConfig();
                SaveToFile(filePath, defaultConfig);
                return defaultConfig;
            }

            string jsonString = File.ReadAllText(filePath);
            var jsonDocument = JsonDocument.Parse(jsonString);
            
            var config = new MenuConfig();
            
            if (jsonDocument.RootElement.TryGetProperty("rounds", out var roundsElement))
            {
                config.Rounds = ParseMenuSection(roundsElement);
            }
            
            if (jsonDocument.RootElement.TryGetProperty("choices", out var choicesElement))
            {
                config.Choices = ParseMenuSection(choicesElement);
            }
            
            if (jsonDocument.RootElement.TryGetProperty("hints", out var hintsElement))
            {
                config.Hints = ParseMenuSection(hintsElement);
            }
            
            if (jsonDocument.RootElement.TryGetProperty("difficulty", out var difficultyElement))
            {
                config.Difficulty = ParseMenuSection(difficultyElement);
            }
            
            if (jsonDocument.RootElement.TryGetProperty("graphics", out var graphicsElement))
            {
                config.Graphics = ParseMenuSection(graphicsElement);
            }

            return config;
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintMessage($"Error loading menu config: {ex.Message}. Using default values.");
            return CreateDefaultConfig();
        }
    }

    private static Dictionary<string, object> ParseMenuSection(JsonElement element)
    {
        var result = new Dictionary<string, object>();
        
        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Number)
            {
                result[property.Name] = property.Value.GetInt32();
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                result[property.Name] = property.Value.GetString() ?? "";
            }
        }
        
        return result;
    }

    public static void SaveToFile(string filePath, MenuConfig config)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            string jsonString = JsonSerializer.Serialize(new
            {
                rounds = config.Rounds,
                choices = config.Choices,
                hints = config.Hints,
                difficulty = config.Difficulty,
                graphics = config.Graphics
            }, options);
            
            File.WriteAllText(filePath, jsonString);
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintMessage($"Error saving menu config: {ex.Message}");
        }
    }

    private static MenuConfig CreateDefaultConfig()
    {
        return new MenuConfig
        {
            Rounds = new Dictionary<string, object>
            {
                {"3 rondas - Juego rápido", 3},
                {"5 rondas - Juego estándar", 5},
                {"7 rondas - Juego largo", 7},
                {"10 rondas - Aventura épica", 10},
                {"✏️ Otro - Personalizar", "custom"}
            },
            Choices = new Dictionary<string, object>
            {
                {"2 opciones - Fácil", 2},
                {"3 opciones - Normal", 3},
                {"4 opciones - Difícil", 4},
                {"5 opciones - Experto", 5},
                {"✏️ Otro - Personalizar", "custom"}
            },
            Hints = new Dictionary<string, object>
            {
                {"Sin pistas - Modo hardcore", 0},
                {"1 pista - Mínima ayuda", 1},
                {"2 pistas - Ayuda moderada", 2},
                {"3 pistas - Ayuda completa", 3},
                {"✏️ Otro - Personalizar", "custom"}
            },
            Difficulty = new Dictionary<string, object>
            {
                {"Fácil - Opciones obvias", "easy"},
                {"Normal - Balance perfecto", "medium"},
                {"Difícil - Desafío real", "hard"},
                {"Extremo - Solo para expertos", "extreme"},
                {"✏️ Otro - Personalizar", "custom"}
            },
            Graphics = new Dictionary<string, object>
            {
                {"🎨 Ilustración - Estilo artístico", "illustration"},
                {"📷 Realista - Fotografía", "realistic"},
                {"🕹️ Retro - Pixel Art 8-bit", "8 bit pixel art"},
                {"🖼️ Anime - Estilo japonés", "anime style"},
                {"🌟 Fantasy - Arte fantástico", "fantasy art"},
                {"✏️ Otro - Personalizar", "custom"}
            }
        };
    }
}