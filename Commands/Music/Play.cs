using System.Text;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using SQR.Models.Music;
using SQR.Translation;

namespace SQR.Commands.Music;

public partial class Music
{
    [SlashCommand("play", "Add track to queue")]
    public async Task PlayCommand(InteractionContext context, [Option("name", "Music to search", false)] string search,
        [Option("SearchSource", "Use different search engine")]
        SearchSources source = SearchSources.YouTube)
    {
        var scope = context.Services.CreateScope();
        var translator = scope.ServiceProvider.GetService<Translator>();
        
        var isSlavic = translator!.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

        var language = translator.Languages[Translator.LanguageCode.EN].Music;

        if (translator!.LocaleMap.ContainsKey(context.Locale))
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

        var searchMap = new Dictionary<SearchSources, LavalinkSearchType>
        {
            { SearchSources.YouTube, LavalinkSearchType.Youtube },
            { SearchSources.AppleMusic, LavalinkSearchType.AppleMusic },
            { SearchSources.Spotify, LavalinkSearchType.Spotify },
            { SearchSources.SoundCloud, LavalinkSearchType.SoundCloud }
        };

        Uri.TryCreate(search, UriKind.Absolute, out var uri);

        LavalinkLoadResult loadResult;

        if (uri is not null)
        {
            loadResult = await node.Rest.GetTracksAsync(uri);
        }
        else
        {
            loadResult = await node.Rest.GetTracksAsync(search, searchMap[source]);
        }
        
        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder
                {
                    IsEphemeral = true,
                    Content = string.Format(language.PlayCommand.TrackSearchFailed, search)
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
            if (!search.Contains(keyword) || uri is not null)
            {
                if (index == playlistKeywords.Length - 1 || uri is not null)
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
                            Content = string.Format(language.PlayCommand.AddedToQueue, 
                                track.Title, track.Author, track.Length.ToString(@"hh\:mm\:ss"))
                        });
                    break;
                }
                continue;
            }
            
            StringBuilder stringBuilder;
            
            if (isSlavic)
            {
                var parts = language.SlavicParts;
                stringBuilder = new StringBuilder(string.Format(language.PlayCommand.PlaylistAddedToQueue,
                    loadResult.Tracks.Count, 
                    translator.WordForSlavicLanguage(loadResult.Tracks.Count, parts.OneTrack, parts.TwoTracks, parts.FiveTracks),
                    loadResult.PlaylistInfo.Name));
            }
            else
            {
                stringBuilder = new StringBuilder(string.Format(language.PlayCommand.PlaylistAddedToQueue,
                    loadResult.Tracks.Count, loadResult.PlaylistInfo.Name));
            }
            foreach (var loadResultTrack in loadResult.Tracks)
            {
                var content = string.Format(language.PlayCommand.AddedToQueueMessagePattern, loadResultTrack.Title, loadResultTrack.Length.ToString(@"hh\:mm\:ss"), loadResultTrack.Author);
                
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
        
        language = translator.Languages[translator.LocaleMap[context.GuildLocale]].Music;

        while (conn.IsConnected && _servers.ContainsKey(conn) && queueCreated)
        {
            if (conn.CurrentState.CurrentTrack != null || !_servers[conn].Queue.Any()) continue;
            
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
                    await context.Channel.SendMessageAsync(language.PlayCommand.EmptyQueue);
                }
            };
                
            await context.Channel.SendMessageAsync(string.Format(language.PlayCommand.NowPlaying, toPlay.LavalinkTrack.Title, toPlay.LavalinkTrack.Author, toPlay.LavalinkTrack.Length.ToString(@"hh\:mm\:ss"))
                                                   + $"{(conn.CurrentState.CurrentTrack?.SourceName == "spotify" ? language.PlayCommand.IfPlaybackStopped : "")}");
            await Task.Delay(1000);
        }
    }
}
