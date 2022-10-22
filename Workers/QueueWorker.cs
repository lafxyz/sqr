using System.ComponentModel;
using System.Data;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Common;
using DisCatSharp.Entities;
using DisCatSharp.Lavalink;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SQR.Commands.Music;
using SQR.Extenstions;
using SQR.Models.Music;
using SQR.Translation;

namespace SQR.Workers;

public class QueueWorker
{
    public static Dictionary<LavalinkGuildConnection, ConnectedGuild> Servers => _servers;
    
    public static readonly string[] PlaylistKeywords = { "/album/", "/playlist/", "/artist/", "list=" };
    
    private static Dictionary<LavalinkGuildConnection, ConnectedGuild> _servers = new();

    public QueueWorker()
    {
        Task.Run(async delegate
        {
            while (true)
            {
                foreach (var (connection, connectedGuild) in _servers)
                {
                    await Task.Run(async delegate
                    {
                        try
                        {
                            var context = connectedGuild.Context;
                            var scope = context.Services.CreateScope();
                            var translator = scope.ServiceProvider.GetService<Translator>();
                            
                            var language = translator!.Languages[Translator.LanguageCode.EN].Music;
                            var isSlavic = translator.Languages[Translator.LanguageCode.EN].IsSlavicLanguage;

                            if (translator.LocaleMap.ContainsKey(context.Locale))
                            {
                                language = translator.Languages[translator.LocaleMap[context.Locale]].Music;
                                isSlavic = translator.Languages[translator.LocaleMap[context.Locale]].IsSlavicLanguage;
                            }

                            if (connection.IsConnected == false)
                            {
                                await DisconnectAsync(connection);
                                return;
                            }

                            if (connection.CurrentState.CurrentTrack != null)
                            {
                                return;
                            }

                            if (connectedGuild.Queue.Any() == false)
                            {
                                if (connectedGuild.FirstTrackRecieved == false) return;
                                const int seconds = 60;
                                if (isSlavic)
                                {
                                    await context.Channel.SendMessageAsync(string.Format(language.PlayCommand.LeavingInNSeconds, seconds,
                                        translator.WordForSlavicLanguage(seconds,
                                            language.SlavicParts.OneSecond,
                                            language.SlavicParts.TwoSeconds,
                                            language.SlavicParts.FiveSeconds)));
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync(string.Format(language.PlayCommand.LeavingInNSeconds, seconds));
                                }

                                await Task.Delay(seconds * 1000);

                                if (connectedGuild.Queue.Any() == false)
                                {
                                    await DisconnectAsync(connection);
                                    await context.Channel.SendMessageAsync(language.PlayCommand.EmptyQueue);
                                }
                            }
                            else
                            {
                                var toPlay = connectedGuild.Queue.First();
                                connectedGuild.Queue.Remove(toPlay);

                                if (connectedGuild.Looping == LoopingState.LoopTrack) connectedGuild.Queue.Insert(0, toPlay);
                                if (connectedGuild.Looping == LoopingState.LoopQueue) connectedGuild.Queue.Add(toPlay);

                                await connection.PlayAsync(toPlay.LavalinkTrack);
                                if (connectedGuild.FirstTrackRecieved == false) connectedGuild.FirstTrackRecieved = true;

                                await context.Channel.SendMessageAsync(string.Format(language.PlayCommand.NowPlaying, toPlay.LavalinkTrack.Title, toPlay.LavalinkTrack.Author, toPlay.LavalinkTrack.Length.ToString(@"hh\:mm\:ss"))
                                                                           + $"{(connection.CurrentState.CurrentTrack?.SourceName == "spotify" ? language.PlayCommand.IfPlaybackStopped : "")}");
                                await Task.Delay(1000);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
            }
        });
    }   
    
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
    
    public enum LoopingState
    {
        [ChoiceName("Without looping")]
        NoLoop,
        [ChoiceName("Loop current track")]
        LoopTrack,
        [ChoiceName("Loop current queue")]
        LoopQueue
    }
    
    public async Task<LoadResult> AddAsync(InteractionContext context, string search, SearchSources source)
    {
        Uri.TryCreate(search, UriKind.Absolute, out var uri);
        
        var searchMap = new Dictionary<SearchSources, LavalinkSearchType>
        {
            { SearchSources.YouTube, LavalinkSearchType.Youtube },
            { SearchSources.AppleMusic, LavalinkSearchType.AppleMusic },
            { SearchSources.Spotify, LavalinkSearchType.Spotify },
            { SearchSources.SoundCloud, LavalinkSearchType.SoundCloud }
        };
        
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Guild);
        
        LavalinkLoadResult lavalinkLoadResult;

        if (uri is not null)
        {
            lavalinkLoadResult = await node.Rest.GetTracksAsync(uri);
        }
        else
        {
            lavalinkLoadResult = await node.Rest.GetTracksAsync(search, searchMap[source]);
        }
        
        if (lavalinkLoadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            if (Servers[connection].Queue.Any() == false) await DisconnectAsync(connection);
        }

        for (var index = 0; index < PlaylistKeywords.Length; index++)
        {
            var keyword = PlaylistKeywords[index];
            if (!search.Contains(keyword) || uri is null)
            {
                if (index == PlaylistKeywords.Length - 1 || uri is null)
                {
                    var track = lavalinkLoadResult.Tracks.First();
                    _servers[connection].Queue.Add(new Track
                    {
                        LavalinkTrack = track,
                        DiscordUser = context.User
                    });

                    _servers[connection].FirstTrackRecieved = true;
                    
                    return new LoadResult()
                    {
                        IsPlaylist = false,
                        LavalinkLoadResult = lavalinkLoadResult
                    };;
                }
            }
        }
        
        foreach (var loadResultTrack in lavalinkLoadResult.Tracks)
        {
            _servers[connection].Queue.Add(new Track
            {
                LavalinkTrack = loadResultTrack,
                DiscordUser = context.User
            });
        }
        
        _servers[connection].FirstTrackRecieved = true;

        return new LoadResult()
        {
            IsPlaylist = true,
            LavalinkLoadResult = lavalinkLoadResult
        };
    }
    
    public async Task<bool> TryConnectAsync(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        if (connection is null)
        {
            connection = await node.ConnectAsync(context.Member.VoiceState?.Channel);
            if (!_servers.ContainsKey(connection))
            {
                _servers.Add(connection, new ConnectedGuild
                {
                    Looping = LoopingState.NoLoop,
                    Queue = new List<Track>(),
                    Context = context,
                    Volume = 100
                });
            }
            return true;
        }
        
        return false;
    }

    public async Task DisconnectAsync(LavalinkGuildConnection connection)
    {
        await connection.DisconnectAsync();
        _servers.Remove(connection);
    }

    public async Task SetVolume(InteractionContext context, int scale)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        await connection.SetVolumeAsync(scale);
        _servers[connection].Volume = scale;
    }
    
    public async Task SetLoopState(InteractionContext context, LoopingState state)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        _servers[connection].Looping = state;
    }

    public async Task Shuffle(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState.Guild);
        
        _servers[connection].Queue = _servers[connection].Queue.OrderBy(x => new SecureRandom().Next()).ToList();
    }

    public async Task<ConnectedGuild?> GetConnectedGuild(InteractionContext context)
    {
        var lava = context.Client.GetLavalink();
        var node = lava.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(context.Member.VoiceState?.Guild);

        return _servers[connection];
    }
    
    
}