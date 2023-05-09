using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Attributes;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("resume", "Resumes playback")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task ResumeCommand(InteractionContext context)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;

        var conn = GetConnection(context);

        await _queue.ResumeAsync(context);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(music.ResumeCommandTranslation.Resumed, conn.CurrentState.CurrentTrack.Title, conn.CurrentState.CurrentTrack.Author)
            });
    }
}