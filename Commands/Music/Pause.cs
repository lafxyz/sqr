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
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);
        
        var conn = GetConnection(context);
        
        await _queue.PauseAsync(context);

        var currentTrack = conn.CurrentState.CurrentTrack;

        var embed = new DiscordEmbedBuilder()
            .AsSQRDefault(context.Client, language)
            .WithTitle(language.MusicTranslation.PauseCommandTranslation.Success)
            .WithDescription(
                string.Format(language.MusicTranslation.PauseCommandTranslation.Paused, currentTrack.Title, currentTrack.Author));

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
}