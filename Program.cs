using QuickType;

namespace SQR;

class Program
{
    public static void Main(string[] args)
    {
        var config = Config.FromJson(File.ReadAllText("config.json"));
        var bot = new Bot();
        
        bot.Login(config).GetAwaiter().GetResult();
    }
}
