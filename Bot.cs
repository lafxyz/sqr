using System.Reflection;
using System.Threading.Channels;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickType;
using Serilog;
using SQR.Commands.Dev;
using SQR.Database;
using SQR.Services;
using SQR.Translation;
using SQR.Workers;

namespace SQR;

public class Bot
{
    public Config Configuration => _configuration;
    private Config _configuration;

    public Bot(Config configuration)
    {
        _configuration = configuration;
    }

    public async Task Login()
    {
        var discordConfiguration = new DiscordConfiguration
        {
            Token = _configuration.Token,
            AutoReconnect = true,
            LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger)
        };
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<Translator>();
        serviceCollection.AddSingleton<QueueWorker>();
        serviceCollection.AddDbContext<Context>(builder =>
        {
            builder.UseNpgsql(
                    $@"host={_configuration.Postgres.Host};port={_configuration.Postgres.Port};database={_configuration.Postgres.Database};username={_configuration.Postgres.Username};password={_configuration.Postgres.Password}");
        });
        serviceCollection.AddScoped<DatabaseService>();

        var applicationCommandsConfiguration = new ApplicationCommandsConfiguration
        {
            ServiceProvider = serviceCollection.BuildServiceProvider(),
            EnableDefaultHelp = true
        };

        var discord = new DiscordShardedClient(discordConfiguration);

        var lavalinkEndpoint = new ConnectionEndpoint
        {
            Hostname = _configuration.Lavalink.Host,
            Port = _configuration.Lavalink.Port
        };

        var lavalinkConfiguration = new LavalinkConfiguration
        {
            Password = _configuration.Lavalink.Password,
            RestEndpoint = lavalinkEndpoint,
            SocketEndpoint = lavalinkEndpoint
        };

        await discord.StartAsync();
        
        discord.Logger.LogInformation("Connection success! Logged in as {Username}#{Discriminator} ({Id})", discord.CurrentUser.Username, discord.CurrentUser.Discriminator, discord.CurrentUser.Id);

        var lavalink = await discord.UseLavalinkAsync();
        foreach (var extension in lavalink.Values)
        {
            await extension.ConnectAsync(lavalinkConfiguration);
        }
        
        var appCommandModule = typeof(ApplicationCommandsModule);
        var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => appCommandModule.IsAssignableFrom(t) && !t.IsNested).ToList();

        var guildCommands = new List<Type>
        {
            typeof(Dev)
        };
        
        foreach (var discordClient in discord.ShardClients.Values)
        {
            var applicationCommands = discordClient.UseApplicationCommands(applicationCommandsConfiguration);
            
            applicationCommands.SlashCommandExecuted += SlashCommandExecuted;
            applicationCommands.SlashCommandErrored += SlashCommandErrored;
            applicationCommands.ContextMenuExecuted += ContextMenuCommandExecuted;
            applicationCommands.ContextMenuErrored += ContextMenuCommandErrored;

            foreach (var command in commands)
            {
                if (guildCommands.Contains(command))
                {
                    applicationCommands.RegisterGuildCommands(command, _configuration.Guild);
                    discord.Logger.LogInformation("{Command} registered as guild command", command);
                    continue;
                }
                
                applicationCommands.RegisterGlobalCommands(command);
		        discordClient.Logger.LogInformation("{Command} registered as global command", command);
            }
        }
        discord.Logger.LogInformation("Application commands registered successfully");
        
        discord.Ready += (_, _) =>
        {
            var activityIndex = 0;

            var activityTask = new BackgroundTask(TimeSpan.FromSeconds(30));
            activityTask.Start(async () =>
            {
                if (_configuration?.Activities is null) return;
                if (activityIndex >= _configuration?.Activities.Count) activityIndex = 0;
                var activity = _configuration?.Activities[activityIndex];
                if (string.IsNullOrWhiteSpace(activity?.Name)) throw new NullReferenceException($"{nameof(activity)}.{nameof(activity.Name)} cannot be null");
                await discord.UpdateStatusAsync(new DiscordActivity { Name = activity.Name, ActivityType = activity.Type, StreamUrl = activity.StreamUrl });

                activityIndex += 1;
            });
            discord.Logger.LogInformation("Ready to use!");
            return Task.CompletedTask;
        };

        await Task.Delay(-1);
    }

    private static Task SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
    {
        Log.Logger.Information($"Slash command executed: {e.Context.CommandName}");
        return Task.CompletedTask;
    }
    
    private static Task SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        Log.Logger.Error(e.Exception,$"Slash command errored: {e.Exception.Message} | Command name: {e.Context.CommandName} | Interaction ID: {e.Context.InteractionId}");
        return Task.CompletedTask;
    }
    
    private static Task ContextMenuCommandExecuted(ApplicationCommandsExtension sender, ContextMenuExecutedEventArgs e)
    {
        Log.Logger.Information($"Context menu command executed: {e.Context.CommandName}");
        return Task.CompletedTask;
    }
    
    private static Task ContextMenuCommandErrored(ApplicationCommandsExtension sender, ContextMenuErrorEventArgs e)
    {
        Log.Logger.Error(e.Exception,$"Context menu command errored: {e.Exception.Message} | Command name: {e.Context.CommandName} | Interaction ID: {e.Context.InteractionId}");
        return Task.CompletedTask;
    }
}
