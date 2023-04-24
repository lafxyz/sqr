using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Extenstions;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("pause", "Pauses playback")]
    public async Task PauseCommand(InteractionContext context)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).Music;
        
        var conn = GetConnection(context);
        
        await _queue.PauseAsync(context);

        var currentTrack = conn.CurrentState.CurrentTrack;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client)
            .WithTitle(music.PauseCommand.Success)
            .WithDescription(
                string.Format(music.PauseCommand.Paused, currentTrack.Title, currentTrack.Author));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
}