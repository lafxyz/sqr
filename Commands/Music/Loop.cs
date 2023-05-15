using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Attributes;
using SQR.Extenstions;
using SQR.Services;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("loop", "Defines loop mode")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task LoopCommand(InteractionContext context, [Option("mode", "Looping mode")] QueueService.LoopingState state)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);
        var music = language.MusicTranslation;

        var connectedGuild = await _queue.GetConnectedGuild(context);

        if (state == QueueService.LoopingState.Single)
        {
            if (connectedGuild.NowPlaying != null) connectedGuild.Queue.Insert(0, connectedGuild.NowPlaying);
        }
        else if (state == QueueService.LoopingState.Queue)
        {
            if (connectedGuild.NowPlaying != null) connectedGuild.Queue.Add(connectedGuild.NowPlaying);
        }
        
        _queue.SetLoopState(context, state);

        var map = new Dictionary<QueueService.LoopingState, string>
        {
            { QueueService.LoopingState.Disabled, music.LoopCommandTranslation.NoLoop },
            { QueueService.LoopingState.Single, music.LoopCommandTranslation.LoopTrack },
            { QueueService.LoopingState.Queue, music.LoopCommandTranslation.LoopQueue }
        };

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client, language)
            .WithTitle(music.LoopCommandTranslation.Success)
            .WithDescription(map[state]);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .AddEmbed(embed));
    }
}