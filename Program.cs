using DisCatSharp.Entities;
using QuickType;
using Serilog;
using SQR.Translation;

namespace SQR;

class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        var config = Config.FromJson(File.ReadAllText("config.json"));
        var bot = new Bot();

        bot.Login(config).GetAwaiter().GetResult();
    }
}
