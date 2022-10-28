using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Translation;
using SQR.Workers;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("queue", "Display queue")]
    public async Task QueueCommand(InteractionContext context)
    {
        var scope = context.Services.CreateScope();
        var translator = scope.ServiceProvider.GetService<Translator>();
        var queue = scope.ServiceProvider.GetService<QueueWorker>();

        var isSlavic = translator!.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

        var language = translator.Languages[Translator.LanguageCode.EN].Music;

        if (translator!.LocaleMap.ContainsKey(context.Locale))
        {
            language = translator.Languages[translator.LocaleMap[context.Locale]].Music;
            isSlavic = translator.Languages[translator.LocaleMap[context.Locale]].IsSlavicLanguage;
        }
        
        var voiceState = context.Member.VoiceState;

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
        
        StringBuilder stringBuilder;

        var connectedGuild = await queue!.GetConnectedGuild(context);

        if (isSlavic)
        {
            var parts = language.SlavicParts;
            stringBuilder = new StringBuilder(string.Format(language.QueueCommand.NowPlaying, 
                currentTrack.Title, currentTrack.Author,
                conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                currentTrack.Length.ToString(@"hh\:mm\:ss"), connectedGuild.Queue.Count,
                translator.WordForSlavicLanguage(connectedGuild!.Queue.Count, parts.OneTrack, parts.TwoTracks, parts.FiveTracks)
            ));
        }
        else
        {
            stringBuilder = new StringBuilder(string.Format(language.QueueCommand.NowPlaying, 
                currentTrack.Title, currentTrack.Author,
                conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                currentTrack.Length.ToString(@"hh\:mm\:ss"), connectedGuild!.Queue.Count
            ));
        }

        for (var index = 0; index < connectedGuild!.Queue.Count; index++)
        {
            var track = connectedGuild!.Queue[index];
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
