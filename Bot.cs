using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DisCatSharp.VoiceNext;
using Microsoft.Extensions.Logging;
using QuickType;
using SQR.Commands.Music;

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

        var applicationCommandsConfiguration = new ApplicationCommandsConfiguration
        {
            EnableDefaultHelp = true
        };

        var discord = new DiscordClient(discordConfiguration);
        var voiceNext = discord.UseVoiceNext();
        var lavalink = discord.UseLavalink();

        var lavalinkEndpoint = new ConnectionEndpoint
        {
            Hostname = config.Lavalink.Host,
            Port = config.Lavalink.Port
        };

        var lavalinkConfiguration = new LavalinkConfiguration
        {
            Password = config.Lavalink.Password,
            RestEndpoint = lavalinkEndpoint,
            SocketEndpoint = lavalinkEndpoint
        };
        
        var applicationCommands = discord.UseApplicationCommands(applicationCommandsConfiguration);
        applicationCommands.RegisterGuildCommands<Music>(config.Guild);

        await discord.ConnectAsync();
        await lavalink.ConnectAsync(lavalinkConfiguration);

        await Task.Delay(-1);
    }
}