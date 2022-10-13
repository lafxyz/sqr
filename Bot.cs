using System.Diagnostics.Metrics;
using System.Reflection;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using DisCatSharp.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickType;
using Serilog;
using SQR.Commands.Music;
using SQR.Translation;

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
            LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger)
        };

        var applicationCommandsConfiguration = new ApplicationCommandsConfiguration
        {
            ServiceProvider = new ServiceCollection().AddSingleton<Translator>().BuildServiceProvider(),
            EnableDefaultHelp = true
        };

        var discord = new DiscordShardedClient(discordConfiguration);

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

        await discord.StartAsync();
        
        discord.Logger.LogInformation($"Connection success! Logged in as {discord.CurrentUser.Username}#{discord.CurrentUser.Discriminator} ({discord.CurrentUser.Id})");

        var lavalink = await discord.UseLavalinkAsync();
        foreach (var extension in lavalink.Values)
            await extension.ConnectAsync(lavalinkConfiguration);
        
        Type appCommandModule = typeof(ApplicationCommandsModule);
        var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => appCommandModule.IsAssignableFrom(t) && !t.IsNested).ToList();

        foreach (DiscordClient discordClient in discord.ShardClients.Values)
        {
            ApplicationCommandsExtension appCommandShardExtension = discordClient.UseApplicationCommands(applicationCommandsConfiguration);

            // Register event handlers
            appCommandShardExtension.SlashCommandExecuted += SlashCommandExecuted;
            appCommandShardExtension.SlashCommandErrored += SlashCommandErrored;
            appCommandShardExtension.ContextMenuExecuted += ContextMenuCommandExecuted;
            appCommandShardExtension.ContextMenuErrored += ContextMenuCommandErrored;

            foreach (var command in commands)
            {
                appCommandShardExtension.RegisterGlobalCommands(command);
		discordClient.Logger.LogInformation($"{command} registered");
            }
        }
        discord.Logger.LogInformation("Application commands registered successfully");
        
        discord.Ready += async (sender, args) =>
        {
#pragma warning disable CS4014
            //We don't care about result from this task, so we run this in parallel and disable annoying warning
            PresenceLoop(sender);
#pragma warning restore CS4014
            discord.Logger.LogInformation("Ready to use!");
        };

        await Task.Delay(-1);
    }

    private async Task PresenceLoop(DiscordClient client)
    {
        var position = 0;
        while (true)
        {
            if (_configuration.Activities is null) return;
            if (position >= _configuration?.Activities.Count) position = 0;
            var activity = _configuration?.Activities[position];
            if (string.IsNullOrWhiteSpace(activity.Name)) throw new NullReferenceException($"{nameof(activity)}.{nameof(activity.Name)} cannot be null");
            await client.UpdateStatusAsync(new DiscordActivity
            {
                Name = activity.Name,
                ActivityType = activity.Type,
                StreamUrl = activity.StreamUrl
            });

            await Task.Delay(10000);
            position += 1;
        }
    }
    
    private static Task SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
    {
        Log.Logger.Information($"Slash command executed: {e.Context.CommandName}");
        return Task.CompletedTask;
    }
    
    private static Task SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        Log.Logger.Error($"Slash command errored: {e.Exception.Message} | Command name: {e.Context.CommandName} | Interaction ID: {e.Context.InteractionId}");
        return Task.CompletedTask;
    }
    
    private static Task ContextMenuCommandExecuted(ApplicationCommandsExtension sender, ContextMenuExecutedEventArgs e)
    {
        Log.Logger.Information($"Context menu command executed: {e.Context.CommandName}");
        return Task.CompletedTask;
    }
    
    private static Task ContextMenuCommandErrored(ApplicationCommandsExtension sender, ContextMenuErrorEventArgs e)
    {
        Log.Logger.Error($"Context menu command errored: {e.Exception.Message} | Command name: {e.Context.CommandName} | Interaction ID: {e.Context.InteractionId}");
        return Task.CompletedTask;
    }
}
