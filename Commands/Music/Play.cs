using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;

namespace SQR.Commands.Music;

public partial class Music : ApplicationCommandsModule
{
    public enum SearchSources
    {
        [ChoiceName("YouTube")]
        YouTube,
        [ChoiceName("Spotify")]
        Spotify,
        [ChoiceName("AppleMusic")]
        AppleMusic,
        [ChoiceName("SoundCloud")]
        SoundCloud
    }
    
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
        
        if (!_connectionsWithQueue.ContainsKey(conn))
        {
            queueCreated = true;
            _connectionsWithQueue.Add(conn, new List<LavalinkTrack>());  
        }

        var map = new Dictionary<SearchSources, LavalinkSearchType>();
        map.Add(SearchSources.YouTube, LavalinkSearchType.Youtube);
        map.Add(SearchSources.AppleMusic, LavalinkSearchType.AppleMusic);
        map.Add(SearchSources.Spotify, LavalinkSearchType.Spotify);
        map.Add(SearchSources.SoundCloud, LavalinkSearchType.SoundCloud);

        var loadResult = await node.Rest.GetTracksAsync(search, map[source]);
        
        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = $"Track search failed for {search}."
                });
            return;
        }
        
        var track = loadResult.Tracks.First();
        
        _connectionsWithQueue[conn].Add(track);

        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true,
                Content = $"Added to queue `{track.Title}` by `{track.Author}` ({track.Length.ToString(@"hh\:mm\:ss")})."
            });

        while (conn.IsConnected && _connectionsWithQueue.ContainsKey(conn) && queueCreated)
        {
            if (conn.CurrentState.CurrentTrack == null && _connectionsWithQueue[conn].Any())
            {
                var toPlay = _connectionsWithQueue[conn].First();
                _connectionsWithQueue[conn].Remove(toPlay);

                await conn.PlayAsync(toPlay);
                
                await context.Channel.SendMessageAsync($"Now playing `{toPlay.Title}` by `{toPlay.Author}` ({toPlay.Length.ToString(@"hh\:mm\:ss")})."
                + $"{(conn.CurrentState.CurrentTrack?.SourceName == "spotify" ? "\n\nIf playback stopped/skipped immediately that means that track was not found on YouTube by ISRC, use YouTube search instead" : "")}");
            }
            else if (conn.CurrentState.CurrentTrack == null && !_connectionsWithQueue[conn].Any())
            {
                await conn.DisconnectAsync();
                await context.Channel.SendMessageAsync($"Empty queue, leaving 👋🏿");
                _connectionsWithQueue.Remove(conn);
            }
        }
    }
}