using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SQR.Translation;
using TimeSpanParserUtil;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("seek", "Sets playback time")]
    public async Task SeekCommand(InteractionContext context, [Option("time", "Time from which playback starts")] string time)
    {
        var music = Language.GetLanguageOrFallback(_translator, context.Locale).Music;
        
        TimeSpan timeSpan;
        var isTimeParsed = TimeSpanParser.TryParse(time, out timeSpan);
        
        //TODO: USE EXCEPTION
        if (isTimeParsed == false)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = music.SeekCommand.ParseFailed
                });
            return;
        }

        var conn = GetConnection(context);

        await conn.SeekAsync(timeSpan);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = String.Format(music.SeekCommand.Seeked, timeSpan.ToString(@"hh\:mm\:ss"), conn.CurrentState.CurrentTrack.Title,
                    conn.CurrentState.CurrentTrack.Author)
            });
    }
}