using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Attributes;
using SQR.Exceptions;
using SQR.Translation;
using TimeSpanParserUtil;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("seek", "Sets playback time")]
    [RequireLavalinkConnection]
    [BotAndUserMustBeInTheSameVoiceChannel]
    [UserMustBeInVoiceChannel]
    public async Task SeekCommand(InteractionContext context, [Option("time", "Time from which playback starts")] string time)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).MusicTranslation;

        var isTimeParsed = TimeSpanParser.TryParse(time, out var timeSpan);
        
        if (isTimeParsed == false)
            throw new ParseFailedException(music.SeekCommandTranslation.ParseFailedComment);

        var conn = GetConnection(context);

        await conn.SeekAsync(timeSpan);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = string.Format(music.SeekCommandTranslation.Seeked, 
                    timeSpan.ToString(@"hh\:mm\:ss"), 
                    conn.CurrentState.CurrentTrack.Title,
                    conn.CurrentState.CurrentTrack.Author
                    )
            });
    }
}