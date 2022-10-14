using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Translation;
using TimeSpanParserUtil;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("seek", "Sets playback time")]
    public async Task SeekCommand(InteractionContext context, [Option("time", "Time from which playback starts")] string time)
    {
        var scope = context.Services.CreateScope();
        var translator = scope.ServiceProvider.GetService<Translator>();

        var isSlavic = translator.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

        var language = translator.Languages[Translator.LanguageCode.EN].Music;

        if (translator.LocaleMap.ContainsKey(context.Locale))
        {
            language = translator.Languages[translator.LocaleMap[context.Locale]].Music;
            isSlavic = translator.Languages[translator.LocaleMap[context.Locale]].IsSlavicLanguage;
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
        
        var voiceState = context.Member.VoiceState;
        if (voiceState?.Channel is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.NotInVoice
                });
        }

        if (context.Guild.CurrentMember.VoiceState != null && voiceState?.Channel != context.Guild.CurrentMember.VoiceState.Channel)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.DifferentVoice
                });
            return;
        }
        
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        if (conn == null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.LavalinkIsNotConnected
                });
            return;
        }
        
        await conn.SeekAsync(timeSpan);
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = String.Format(language.SeekCommand.Seeked, timeSpan.ToString(@"hh\:mm\:ss"), conn.CurrentState.CurrentTrack.Title,
                    conn.CurrentState.CurrentTrack.Author)
            });
    }
}