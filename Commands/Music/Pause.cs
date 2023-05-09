using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Attributes;
using SQR.Extenstions;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("pause", "Pauses playback")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task PauseCommand(InteractionContext context)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;
        
        var conn = GetConnection(context);
        
        await _queue.PauseAsync(context);

        var currentTrack = conn.CurrentState.CurrentTrack;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client)
            .WithTitle(music.PauseCommandTranslation.Success)
            .WithDescription(
                string.Format(music.PauseCommandTranslation.Paused, currentTrack.Title, currentTrack.Author));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
}