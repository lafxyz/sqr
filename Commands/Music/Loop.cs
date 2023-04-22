using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Extenstions;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("loop", "Defines loop mode")]
    public async Task LoopCommand(InteractionContext context, [Option("mode", "Looping mode")] QueueWorker.LoopingState state)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).Music;

        var connectedGuild = await _queue.GetConnectedGuild(context);

        if (state == QueueWorker.LoopingState.Single)
        {
            if (connectedGuild.NowPlaying != null) connectedGuild.Queue.Insert(0, connectedGuild.NowPlaying);
        }
        else if (state == QueueWorker.LoopingState.Queue)
        {
            if (connectedGuild.NowPlaying != null) connectedGuild.Queue.Add(connectedGuild.NowPlaying);
        }
        
        _queue.SetLoopState(context, state);

        var map = new Dictionary<QueueWorker.LoopingState, string>
        {
            { QueueWorker.LoopingState.Disabled, music.LoopCommand.NoLoop },
            { QueueWorker.LoopingState.Single, music.LoopCommand.LoopTrack },
            { QueueWorker.LoopingState.Queue, music.LoopCommand.LoopQueue }
        };

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault()
            .WithTitle(music.LoopCommand.Success)
            .WithDescription(map[state]);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
    }
}