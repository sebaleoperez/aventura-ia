public sealed class CommandLineOptions
{
    public string? Language { get; private init; }
    public bool GenerateVideo { get; private init; }
    public bool GenerateImages { get; private init; } = true;
    public bool TextOnly { get; private init; }

    public static CommandLineOptions Parse(string[] args)
    {
        bool textOnly = args.Contains("--text-only");

        return new CommandLineOptions
        {
            Language = args.FirstOrDefault(arg => !arg.StartsWith("--")),
            TextOnly = textOnly,
            GenerateVideo = !textOnly && args.Contains("--video"),
            GenerateImages = !textOnly && !args.Contains("--no-images")
        };
    }
}
