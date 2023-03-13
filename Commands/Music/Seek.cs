using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Expections;
using SQR.Translation;
using TimeSpanParserUtil;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("seek", "Sets playback time")]
    public async Task SeekCommand(InteractionContext context, [Option("time", "Time from which playback starts")] string time)
    {
        var language = Translator.Languages[Translator.FallbackLanguage].Music;

        if (Translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
        }
        
        TimeSpan timeSpan;
        var isTimeParsed = TimeSpanParser.TryParse(time, out timeSpan);
        
        if (isTimeParsed == false)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.SeekCommand.ParseFailed
                });
            return;
        }

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        
        
        await conn.SeekAsync(timeSpan);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = String.Format(language.SeekCommand.Seeked, timeSpan.ToString(@"hh\:mm\:ss"), conn.CurrentState.CurrentTrack.Title,
                    conn.CurrentState.CurrentTrack.Author)
            });
    }
}