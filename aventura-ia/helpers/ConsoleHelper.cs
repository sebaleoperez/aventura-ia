using System.Diagnostics;

public class ConsoleHelper {
    public static void Clear()
    {
        Console.Clear();
    }
    public static void PrintMessage(string message)
    {
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
    }

    public static void PrintColoredMessage(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        PrintMessage(message);
        Console.ResetColor();
    }
    public static string? ReadData(string message)
    {
        Console.WriteLine(message);
        string? data = Console.ReadLine();
        Console.WriteLine();
        return data;
    }

    public static void PrintTitle(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("************************************");
        Console.WriteLine();
        Console.WriteLine(message);
        Console.WriteLine();
        Console.WriteLine("************************************");
        Console.WriteLine();
        Console.ResetColor();
    }

    public static void OpenImage(Uri imageUri) {
        Process.Start(new ProcessStartInfo(imageUri.ToString()){ UseShellExecute = true });
    }

    public static T SelectFromMenu<T>(string prompt, Dictionary<string, T> options)
    {
        Console.WriteLine(prompt);
        Console.WriteLine();

        var optionsList = options.ToList();
        int selectedIndex = 0;

        while (true)
        {
            // Mostrar todas las opciones
            for (int i = 0; i < optionsList.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"► {optionsList[i].Key}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"  {optionsList[i].Key}");
                }
            }

            // Leer la tecla presionada
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = selectedIndex > 0 ? selectedIndex - 1 : optionsList.Count - 1;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = selectedIndex < optionsList.Count - 1 ? selectedIndex + 1 : 0;
                    break;
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return optionsList[selectedIndex].Value;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
            }

            // Limpiar la pantalla para redibujar el menú
            Console.SetCursorPosition(0, Console.CursorTop - optionsList.Count);
            for (int i = 0; i < optionsList.Count; i++)
            {
                Console.WriteLine(new string(' ', Console.WindowWidth - 1));
            }
            Console.SetCursorPosition(0, Console.CursorTop - optionsList.Count);
        }
    }

    public static T SelectFromMenuWithCustom<T>(string prompt, Dictionary<string, object> options, string customPrompt, Func<string, T> customParser)
    {
        var result = SelectFromMenu(prompt, options);
        
        if (result != null && result.ToString() == "custom")
        {
            string? customInput = ReadData(customPrompt);
            return customParser(customInput!);
        }
        
        // Conversión segura de tipos
        if (result is T directResult)
        {
            return directResult;
        }
        
        // Para conversiones de números
        if (typeof(T) == typeof(ushort) && result is int intValue)
        {
            return (T)(object)(ushort)intValue;
        }
        
        // Para conversiones de string
        if (typeof(T) == typeof(string))
        {
            return (T)(object)result!.ToString()!;
        }
        
        // Fallback - intentar conversión directa
        return (T)Convert.ChangeType(result!, typeof(T));
    }
}