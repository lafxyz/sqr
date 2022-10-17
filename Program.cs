using QuickType;
using Serilog;

namespace SQR;

class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("output.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        var config = Config.FromJson(File.ReadAllText("config.json"));
        var bot = new Bot();

        bot.Login(config).GetAwaiter().GetResult();
    }
}
