using System.Reflection;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.Lavalink;
using DisCatSharp.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SQR.Database;
using SQR.Exceptions;
using SQR.Services;
using SQR.Translation;
using SQR.Utilities;

namespace SQR;

public class Bot
{
    public static Config Config => _config;
    private static Config _config;

    public Bot(Config config)
    {
        _config = config;
    }

    public async Task Login()
    {
        var discordConfiguration = new DiscordConfiguration
        {
            Token = _config.Token,
            AutoReconnect = true,
            LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger),
            Intents = DiscordIntents.All
        };
        var postgresHost = _config.Docker.Enabled ? _config.Docker.PostgresHost : _config.Postgres.Host;
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<Translator>();
        serviceCollection.AddSingleton<QueueService>();
        serviceCollection.AddSingleton<ExceptionHandler>();
        serviceCollection.AddDbContext<Context>(builder =>
        {
            builder.UseNpgsql(
                    $@"host={postgresHost};port={_config.Postgres.Port};database={_config.Postgres.Database};username={_config.Postgres.Username};password={_config.Postgres.Password}");
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
            Hostname = _config.Docker.Enabled ? _config.Docker.LavalinkHost : _config.Lavalink.Host,
            Port = _config.Lavalink.Port
        };

        var lavalinkConfiguration = new LavalinkConfiguration
        {
            Password = _config.Lavalink.Password,
            RestEndpoint = lavalinkEndpoint,
            SocketEndpoint = lavalinkEndpoint
        };

        await discord.StartAsync();
        
        discord.Logger.LogInformation("Connection success! Logged in as {Username}#{Discriminator} ({Id})",
            discord.CurrentUser.Username, discord.CurrentUser.Discriminator, discord.CurrentUser.Id);

        var lavalink = await discord.UseLavalinkAsync();
        foreach (var extension in lavalink.Values)
        {
            await extension.ConnectAsync(lavalinkConfiguration);
        }
        
        var appCommandModule = typeof(ApplicationCommandsModule);
        var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => 
            appCommandModule.IsAssignableFrom(t) && !t.IsNested).ToList();

        var guildCommands = new List<Type>
        {
            typeof(DevTranslation)
        };
        
        foreach (var discordClient in discord.ShardClients.Values)
        {
            discordClient.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            
            var applicationCommands = discordClient.UseApplicationCommands(applicationCommandsConfiguration);
            
            applicationCommands.SlashCommandExecuted += SlashCommandExecuted;
            applicationCommands.SlashCommandErrored += SlashCommandErrored;
            applicationCommands.ContextMenuExecuted += ContextMenuCommandExecuted;
            applicationCommands.ContextMenuErrored += ContextMenuCommandErrored;

            foreach (var command in commands)
            {
                if (guildCommands.Contains(command))
                {
                    applicationCommands.RegisterGuildCommands(command, _config.Guild);
                    discord.Logger.LogInformation("{Command} registered as guild command", command);
                    continue;
                }
                
                applicationCommands.RegisterGlobalCommands(command);
		        discordClient.Logger.LogInformation("{Command} registered as global command", command);
            }
        }
        discord.Logger.LogInformation("Application commands registered successfully");

        discord.VoiceStateUpdated += QueueService.VoiceStateUpdate;
        
        discord.Ready += DiscordOnReady;

        await Task.Delay(-1);
    }

    private static async Task DiscordOnReady(DiscordClient client, ReadyEventArgs e)
    {
        _config.DeveloperUser = await client.GetUserAsync(_config.DeveloperId, true);

        var configUpdater = new BackgroundTask(TimeSpan.FromHours(12));
        configUpdater.AssignAndStartTask(async () =>
        {
            _config.DeveloperUser = await client.GetUserAsync(_config.DeveloperId, true);
        });

        var activityIndex = 0;

        var activityTask = new BackgroundTask(TimeSpan.FromSeconds(30));
        activityTask.AssignAndStartTask(async () =>
        {
            if (activityIndex >= _config.Activities.Count) activityIndex = 0;
            var activity = _config.Activities[activityIndex];
            if (string.IsNullOrWhiteSpace(activity?.Name)) 
                throw new NullReferenceException($"{nameof(activity)}.{nameof(activity.Name)} cannot be null");
            await client.UpdateStatusAsync(new DiscordActivity { Name = activity.Name, ActivityType = activity.Type, StreamUrl = activity.StreamUrl });

            activityIndex += 1;
        });
        client.Logger.LogInformation("Ready to use!");
    }

    private static Task SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
    {
        Log.Logger.Information($"Slash command executed: {e.Context.CommandName}");
        return Task.CompletedTask;
    }
    
    private static async Task SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        var scope = e.Context.Services.CreateScope();
        var exceptionHandler = scope.ServiceProvider.GetService<ExceptionHandler>();
        await exceptionHandler!.HandleSlashException(sender, e);
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
