using Serilog;
using SQR.Translation;

namespace SQR;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/output.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var config = Config.FromJson(await File.ReadAllTextAsync("config.json"));

        if (config.Docker.Enabled)
        {
            Log.Logger.Information("Docker enabled, waiting {StartupDelay} seconds to startup", 
                config.Docker.StartupDelay);
            await Task.Delay(TimeSpan.FromSeconds(config.Docker.StartupDelay));
        }

        Translator.FallbackLanguage = config.FallbackLanguage;
        var bot = new Bot(config);
        bot.Login().GetAwaiter().GetResult();
    }
}
