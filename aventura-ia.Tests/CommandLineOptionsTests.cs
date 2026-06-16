namespace aventura_ia.Tests;

public class CommandLineOptionsTests
{
    [Fact]
    public void Parse_UsesFirstNonFlagArgumentAsLanguage()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["spanish", "--video"]);

        Assert.Equal("spanish", options.Language);
        Assert.True(options.GenerateVideo);
    }

    [Fact]
    public void Parse_TextOnlyDisablesImagesAndVideo()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["english", "--video", "--text-only"]);

        Assert.Equal("english", options.Language);
        Assert.True(options.TextOnly);
        Assert.False(options.GenerateVideo);
        Assert.False(options.GenerateImages);
    }

    [Fact]
    public void Parse_NoImagesKeepsVideoEnabledWhenRequested()
    {
        CommandLineOptions options = CommandLineOptions.Parse(["--no-images", "--video"]);

        Assert.Null(options.Language);
        Assert.False(options.GenerateImages);
        Assert.True(options.GenerateVideo);
    }
}
