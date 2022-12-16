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
        var isSlavic = Translator.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

        var language = Translator.Languages[Translator.LanguageCode.EN].Music;

        if (Translator!.LocaleMap.ContainsKey(context.Locale))
        {
            language = Translator.Languages[Translator.LocaleMap[context.Locale]].Music;
            isSlavic = Translator.Languages[Translator.LocaleMap[context.Locale]].IsSlavicLanguage;
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
        
        StringBuilder stringBuilder = new StringBuilder();

        var connectedGuild = await Queue.GetConnectedGuild(context);
        
        var tracks = connectedGuild.Queue;
        
        TimeSpan estimatedPlaybackTime = default;

        if (tracks != null)
        {
            estimatedPlaybackTime = tracks.Aggregate(estimatedPlaybackTime,
                (current, track) => current.Add(track.LavalinkTrack.Length));  

            if (isSlavic)
            {
                var parts = language.SlavicParts;

                stringBuilder = new StringBuilder(string.Format(language.QueueCommand.NowPlaying,
                    currentTrack.Title, currentTrack.Author,
                    conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                    currentTrack.Length.ToString(@"hh\:mm\:ss"), tracks.Count,
                    estimatedPlaybackTime.ToString(@"hh\:mm\:ss"),
                    Translator.WordForSlavicLanguage(tracks.Count, parts.OneTrack, parts.TwoTracks,
                        parts.FiveTracks)
                ));
            }
            else
            {
                stringBuilder = new StringBuilder(string.Format(language.QueueCommand.NowPlaying,
                    currentTrack.Title, currentTrack.Author,
                    conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                    currentTrack.Length.ToString(@"hh\:mm\:ss"), tracks.Count, estimatedPlaybackTime.ToString(@"hh\:mm\:ss")
                ));
            }
        }

        if (connectedGuild.Queue != null)
            for (var index = 0; index < connectedGuild.Queue.Count; index++)
            {
                var track = connectedGuild!.Queue[index];
                var lavalinkTrack = track.LavalinkTrack;
                var content = string.Format(language.QueueCommand.QueueMessagePattern, lavalinkTrack.Author,
                    lavalinkTrack.Title, lavalinkTrack.Length.ToString(@"hh\:mm\:ss"), track.DiscordUser.Mention);
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
