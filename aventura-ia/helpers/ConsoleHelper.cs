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
}