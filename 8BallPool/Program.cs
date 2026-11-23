namespace EightBallPool;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Applies the high-DPI, visual styles and text rendering settings declared in the .csproj.
        ApplicationConfiguration.Initialize();
        Application.Run(new FormMain());
    }
}
