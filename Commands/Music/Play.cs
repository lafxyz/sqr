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
        [Option("SearchSource", "Use different search engine")] SearchSources source = SearchSources.YouTube)
    {
        var voiceState = context.Member.VoiceState;
        if (voiceState is null)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = "You're not in voice channel"
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
                Queue = new List<LavalinkTrack>()
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
                    Content = $"Track search failed for {search}."
                });
            await DisconnectAsync(conn);
            return;
        }
        
        var track = loadResult.Tracks.First();
        
        _servers[conn].Queue.Add(track);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Added to queue `{track.Title}` by `{track.Author}` ({track.Length.ToString(@"hh\:mm\:ss")})."
            });

        while (conn.IsConnected && _servers.ContainsKey(conn) && queueCreated)
        {
            if (conn.CurrentState.CurrentTrack == null && _servers[conn].Queue.Any())
            {
                var toPlay = _servers[conn].Queue.First();
                _servers[conn].Queue.Remove(toPlay);

                await conn.PlayAsync(toPlay);

                conn.PlaybackFinished += async (sender, args) =>
                {
                    if (_servers[conn].Looping == LoopingState.LoopTrack) _servers[conn].Queue.Insert(0, toPlay);
                    if (_servers[conn].Looping == LoopingState.LoopQueue) _servers[conn].Queue.Add(toPlay);
                    
                    if (conn.CurrentState.CurrentTrack == null && !_servers[conn].Queue.Any())
                    {
                        await DisconnectAsync(conn);
                        await context.Channel.SendMessageAsync($"Empty queue, leaving 👋🏿");
                    }
                };
                
                await context.Channel.SendMessageAsync($"Now playing `{toPlay.Title}` by `{toPlay.Author}` ({toPlay.Length.ToString(@"hh\:mm\:ss")})."
                + $"{(conn.CurrentState.CurrentTrack?.SourceName == "spotify" ? "\n\nIf playback stopped/skipped immediately that means that track was not found on YouTube by ISRC, use YouTube search instead" : "")}");
            }
        }
    }
}