using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("stop", "Stops playback and leaves from channel")]
    public async Task StopCommand(InteractionContext context)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;
        
        var voiceState = context.Member.VoiceState;

        var conn = GetConnection(context);

        await _queue.DisconnectAsync(conn);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(music.StopCommandTranslation.Disconnected, voiceState.Channel.Name)
            });
    }
}
