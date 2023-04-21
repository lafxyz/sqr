using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("queue", "Display queue")]
    public async Task QueueCommand(InteractionContext context)
    {
        var language = Language.GetLanguageOrFallback(_translator, context.Locale);
        var music = language.Music;
        
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState?.Guild);

        var currentTrack = conn.CurrentState.CurrentTrack;
        
        StringBuilder stringBuilder = new StringBuilder();

        var connectedGuild = await _queue.GetConnectedGuild(context);
        
        var tracks = connectedGuild.Queue;
        
        TimeSpan estimatedPlaybackTime = default;

        estimatedPlaybackTime = tracks.Aggregate(estimatedPlaybackTime,
            (current, track) => current.Add(track.LavalinkTrack.Length));  

        if (language.IsSlavicLanguage)
        {
            var parts = music.SlavicParts;

            stringBuilder = new StringBuilder(string.Format(music.QueueCommand.NowPlaying,
                currentTrack.Title, 
                currentTrack.Author,
                conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                currentTrack.Length.ToString(@"hh\:mm\:ss"), 
                tracks.Count,
                estimatedPlaybackTime.ToString(@"hh\:mm\:ss"),
                Translator.WordForSlavicLanguage(tracks.Count, parts.OneTrack, parts.TwoTracks,
                    parts.FiveTracks)
            ));
        }
        else
        {
            stringBuilder = new StringBuilder(string.Format(music.QueueCommand.NowPlaying,
                currentTrack.Title, currentTrack.Author,
                conn.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss"),
                currentTrack.Length.ToString(@"hh\:mm\:ss"), tracks.Count, estimatedPlaybackTime.ToString(@"hh\:mm\:ss")
            ));
        }

        for (var index = 0; index < connectedGuild.Queue.Count; index++)
        {
            var track = connectedGuild!.Queue[index];
            var lavalinkTrack = track.LavalinkTrack;
            var content = string.Format(music.QueueCommand.QueueMessagePattern, lavalinkTrack.Author,
                lavalinkTrack.Title, lavalinkTrack.Length.ToString(@"hh\:mm\:ss"), track.DiscordUser.Mention);
            if (stringBuilder.Length + content.Length <= 2000)
            {
                stringBuilder.Append(content);
            }
        }

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                Content = stringBuilder.ToString()
            });
    }
}
