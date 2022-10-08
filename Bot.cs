using DisCatSharp;
using DisCatSharp.Enums;
using Microsoft.Extensions.Logging;
using QuickType;

namespace SQR;

public class Bot
{
    public Config? Configuration => _configuration;
    private Config? _configuration;
    
    public async Task Login(Config? config)
    {
        _configuration = config;

        var discordConfiguration = new DiscordConfiguration
        {
            Token = config.Token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            AutoReconnect = true
        };

        var discord = new DiscordClient(discordConfiguration);
        
        await discord.ConnectAsync();

        await Task.Delay(-1);
    }
}