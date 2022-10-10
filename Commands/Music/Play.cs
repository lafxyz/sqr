using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using SQR.Models.Music;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("play", "Add track to queue")]
    public async Task PlayCommand(InteractionContext context, [Option("name", "Music to search", false)] string search,
        [Option("SearchSource", "Use different search engine")]
        SearchSources source = SearchSources.YouTube)
    {
        var voiceState = context.Member.VoiceState;
        if (voiceState is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = ":x: You're not in voice channel!"
                });
            return;
        }

        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var conn = node.GetGuildConnection(context.Member.VoiceState.Guild);

        if (conn == null)
        {
            conn = await node.ConnectAsync(voiceState.Channel);
        }

        var queueCreated = false;

        if (!_servers.ContainsKey(conn))
        {
            queueCreated = true;
            _servers.Add(conn, new ConnectedGuild
            {
                Looping = LoopingState.NoLoop,
                Queue = new List<Track>()
            });
        }

        var map = new Dictionary<SearchSources, LavalinkSearchType>();
        map.Add(SearchSources.YouTube, LavalinkSearchType.Youtube);
        map.Add(SearchSources.AppleMusic, LavalinkSearchType.AppleMusic);
        map.Add(SearchSources.Spotify, LavalinkSearchType.Spotify);
        map.Add(SearchSources.SoundCloud, LavalinkSearchType.SoundCloud);

        //TODO: Sometimes valid Uri counts as invalid
        var isUriCreated = Uri.TryCreate(search, UriKind.Absolute, out var uri);

        LavalinkLoadResult loadResult;

        if (isUriCreated)
        {
            loadResult = await node.Rest.GetTracksAsync(uri);
        }
        else
        {
            loadResult = await node.Rest.GetTracksAsync(search, map[source]);
        }

        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = $":x: Track search failed for {search}."
                });
            await DisconnectAsync(conn);
            return;
        }

        var playlistKeywords = new[]
        {
            "/album/", "/playlist/", "/artist/", "list="
        };

        for (var index = 0; index < playlistKeywords.Length; index++)
        {
            var keyword = playlistKeywords[index];
            if (!search.Contains(keyword) || isUriCreated == false)
            {
                if (index == playlistKeywords.Length - 1 || isUriCreated == false)
                {
                    var track = loadResult.Tracks.First();
                    _servers[conn].Queue.Add(new Track
                    {
                        LavalinkTrack = track,
                        DiscordUser = context.User
                    });
                    
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder
                        {
                            IsEphemeral = true,
                            Content = $"✅ Added to queue **{track.Title}** by **{track.Author}** `{track.Length.ToString(@"hh\:mm\:ss")}`"
                        });
                    break;
                }
                continue;
            }


            var stringBuilder = new StringBuilder($"✅ Successfully added `{loadResult.Tracks.Count}` tracks from **{loadResult.PlaylistInfo.Name}**:");
            foreach (var loadResultTrack in loadResult.Tracks)
            {
                var content = $"\n**{loadResultTrack.Title}**\n> `{loadResultTrack.Length.ToString(@"hh\:mm\:ss")}` **{loadResultTrack.Author}**\n";
                if (stringBuilder.Length + content.Length <= 2000)
                {
                    stringBuilder.Append(content);
                }
                _servers[conn].Queue.Add(new Track
                {
                    LavalinkTrack = loadResultTrack,
                    DiscordUser = context.User
                });
            }
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = stringBuilder.ToString()
                });
            break;
        }

        while (conn.IsConnected && _servers.ContainsKey(conn) && queueCreated)
        {
            if (conn.CurrentState.CurrentTrack == null && _servers[conn].Queue.Any())
            {
                var toPlay = _servers[conn].Queue.First();
                _servers[conn].Queue.Remove(toPlay);

                await conn.PlayAsync(toPlay.LavalinkTrack);

                conn.PlaybackFinished += async (sender, args) =>
                {
                    if (_servers[conn].Looping == LoopingState.LoopTrack) _servers[conn].Queue.Insert(0, toPlay);
                    if (_servers[conn].Looping == LoopingState.LoopQueue) _servers[conn].Queue.Add(toPlay);
                    
                    if (conn.CurrentState.CurrentTrack == null && !_servers[conn].Queue.Any())
                    {
                        await DisconnectAsync(conn);
                        await context.Channel.SendMessageAsync($"Empty queue, leaving 👋");
                    }
                };
                
                await context.Channel.SendMessageAsync($"ℹ️ Now playing **{toPlay.LavalinkTrack.Title}** by **{toPlay.LavalinkTrack.Author}** `{toPlay.LavalinkTrack.Length.ToString(@"hh\:mm\:ss")}`"
                + $"{(conn.CurrentState.CurrentTrack?.SourceName == "spotify" ? "\n\n⚠️ If playback stopped/skipped immediately that means that track was not found on YouTube by ISRC, use YouTube search instead" : "")}");
                await Task.Delay(1000);
            }
        }
    }
}
