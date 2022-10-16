using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("queue", "Display queue")]
    public async Task QueueCommand(InteractionContext context)
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
        
        var voiceState = context.Member.VoiceState;
        if (voiceState is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = language.General.NotInVoice
                });
        }

        if (context.Guild.CurrentMember.VoiceState != null && voiceState.Channel != context.Guild.CurrentMember.VoiceState.Channel)
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
        var conn = node.GetGuildConnection(context.Member.VoiceState?.Guild);
        
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

        var currentTrack = conn.CurrentState.CurrentTrack;
        
        var stringBuilder = new StringBuilder(string.Format(language.QueueCommand.NowPlaying, 
            currentTrack.Title, currentTrack.Author,
            conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
            currentTrack.Length.ToString(@"hh\:mm\:ss"), _servers[conn].Queue.Count
            ));

        for (var index = 0; index < _servers[conn].Queue.Count; index++)
        {
            var track = _servers[conn].Queue[index];
            var lavalinkTrack = track.LavalinkTrack;
            var content = string.Format(language.QueueCommand.QueueMessagePattern, lavalinkTrack.Author, lavalinkTrack.Title, lavalinkTrack.Length.ToString(@"hh\:mm\:ss"), track.DiscordUser.Mention);
            if (stringBuilder.Length + content.Length <= 2000)
            {
                stringBuilder.Append(content);
            }
        }

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = stringBuilder.ToString()
            });
    }
}
