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

        _queue.SetLoopState(context, state);

        var map = new Dictionary<QueueWorker.LoopingState, string>
        {
            { QueueWorker.LoopingState.NoLoop, music.LoopCommand.NoLoop },
            { QueueWorker.LoopingState.LoopTrack, music.LoopCommand.LoopTrack },
            { QueueWorker.LoopingState.LoopQueue, music.LoopCommand.LoopQueue }
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