namespace Sia.Physics.WebGPU.Example;

public static class Program
{
    public static int Main(string[] args)
    {
        try {
            if (args.Contains("--headless", StringComparer.OrdinalIgnoreCase)) {
                return ExhibitionVerifier.Run(Console.Out, Console.Error);
            }

            var smokeTest = args.Contains("--smoke", StringComparer.OrdinalIgnoreCase);
            using var app = new PhysicsDemoApp(
                maximumFrameCount: smokeTest ? 3 : null,
                visible: !smokeTest);
            app.Run();
            return 0;
        }
        catch (Exception exception) {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
